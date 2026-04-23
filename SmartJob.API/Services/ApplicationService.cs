using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Applications;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IAIMatchService _aiMatch;

    public ApplicationService(AppDbContext db, IMapper mapper, IAIMatchService aiMatch)
    {
        _db = db;
        _mapper = mapper;
        _aiMatch = aiMatch;
    }

    public async Task<ApplicationDto> ApplyAsync(Guid seekerId, ApplyRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.Include(j => j.Employer).FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");
            
        if (job.Status != JobStatus.Active)
            throw new ApiException(StatusCodes.Status400BadRequest, "Cannot apply to a job that is not active.");

        var existingApp = await _db.Applications.FirstOrDefaultAsync(a => a.JobId == request.JobId && a.SeekerId == seekerId, cancellationToken);
        if (existingApp != null)
            throw new ApiException(StatusCodes.Status409Conflict, "You have already applied to this job.");

        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == request.ResumeId && r.SeekerId == seekerId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Resume not found.");

        var app = new Application
        {
            JobId = request.JobId,
            SeekerId = seekerId,
            ResumeId = request.ResumeId,
            Status = ApplicationStatus.Sent,
            AIMatchScore = await _aiMatch.ComputeScoreAsync(request.JobId, seekerId, cancellationToken),
            AppliedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Applications.Add(app);
        
        var notification = new Notification
        {
            UserId = job.EmployerId,
            Type = NotificationType.NewApplicant,
            Title = "New Application Received",
            Message = $"You have a new application for {job.Title}.",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);

        await _db.SaveChangesAsync(cancellationToken);

        return await GetApplicationAsync(app.Id, seekerId, cancellationToken);
    }

    public async Task<PagedResult<ApplicationDto>> GetMyApplicationsAsync(Guid seekerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Applications
            .Include(a => a.Job).ThenInclude(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(a => a.Seeker)
            .Include(a => a.Resume)
            .Where(a => a.SeekerId == seekerId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        
        var apps = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ApplicationDto>
        {
            Items = _mapper.Map<List<ApplicationDto>>(apps),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ApplicationDto>> GetJobApplicationsAsync(Guid jobId, Guid employerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        if (job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own this job.");

        var query = _db.Applications
            .Include(a => a.Job).ThenInclude(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(a => a.Seeker)
            .Include(a => a.Resume)
            .Where(a => a.JobId == jobId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        
        var apps = await query
            .OrderByDescending(a => a.AIMatchScore)
            .ThenByDescending(a => a.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ApplicationDto>
        {
            Items = _mapper.Map<List<ApplicationDto>>(apps),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ApplicationDto> GetApplicationAsync(Guid id, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var app = await _db.Applications
            .Include(a => a.Job).ThenInclude(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(a => a.Seeker)
            .Include(a => a.Resume)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Application not found.");

        if (app.SeekerId != requesterId && app.Job.EmployerId != requesterId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Access denied.");

        return _mapper.Map<ApplicationDto>(app);
    }

    public async Task<ApplicationDto> PatchStatusAsync(Guid applicationId, Guid employerId, string status, CancellationToken cancellationToken = default)
    {
        var app = await _db.Applications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Application not found.");

        if (app.Job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own the job for this application.");

        if (!Enum.TryParse<ApplicationStatus>(status, true, out var newStatus))
            throw new ApiException(StatusCodes.Status400BadRequest, "Invalid status.");

        app.Status = newStatus;
        app.UpdatedAt = DateTime.UtcNow;

        var notification = new Notification
        {
            UserId = app.SeekerId,
            Type = NotificationType.StatusChange,
            Title = "Application Status Updated",
            Message = $"Your application for {app.Job.Title} has been updated to {newStatus}.",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);

        await _db.SaveChangesAsync(cancellationToken);

        return await GetApplicationAsync(app.Id, employerId, cancellationToken);
    }
}
