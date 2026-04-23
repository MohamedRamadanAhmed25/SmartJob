using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Resumes;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class ResumeService : IResumeService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILocalFileStorageService _fileStorageService;

    public ResumeService(AppDbContext dbContext, IMapper mapper, ILocalFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<ResumeDto>> GetMyResumesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureSeekerAsync(userId, cancellationToken);

        return await _dbContext.Resumes
            .Where(r => r.SeekerId == userId)
            .OrderByDescending(r => r.IsDefault)
            .ThenByDescending(r => r.UploadedAt)
            .ProjectTo<ResumeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResumeDto> UploadAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        await EnsureSeekerAsync(userId, cancellationToken);

        var existingDefault = await _dbContext.Resumes.AnyAsync(r => r.SeekerId == userId && r.IsDefault, cancellationToken);
        var (relativeUrl, _) = await _fileStorageService.SaveResumeAsync(file, cancellationToken);

        var resume = new Resume
        {
            SeekerId = userId,
            FileName = file.FileName,
            FileUrl = relativeUrl,
            IsDefault = !existingDefault
        };

        _dbContext.Resumes.Add(resume);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ResumeDto>(resume);
    }

    public async Task DeleteAsync(Guid userId, Guid resumeId, CancellationToken cancellationToken = default)
    {
        await EnsureSeekerAsync(userId, cancellationToken);

        var resume = await _dbContext.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.SeekerId == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Resume not found.");

        var wasDefault = resume.IsDefault;
        _dbContext.Resumes.Remove(resume);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _fileStorageService.DeleteRelativeFile(resume.FileUrl);

        if (wasDefault)
        {
            var nextResume = await _dbContext.Resumes
                .Where(r => r.SeekerId == userId)
                .OrderByDescending(r => r.UploadedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextResume is not null)
            {
                nextResume.IsDefault = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task<ResumeDto> SetDefaultAsync(Guid userId, Guid resumeId, CancellationToken cancellationToken = default)
    {
        await EnsureSeekerAsync(userId, cancellationToken);

        var resumes = await _dbContext.Resumes.Where(r => r.SeekerId == userId).ToListAsync(cancellationToken);
        var target = resumes.FirstOrDefault(r => r.Id == resumeId)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Resume not found.");

        foreach (var resume in resumes)
        {
            resume.IsDefault = resume.Id == resumeId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ResumeDto>(target);
    }

    private async Task EnsureSeekerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "User not found.");

        if (user.Role != UserRole.Seeker)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Only seekers can manage resumes.");
        }
    }
}
