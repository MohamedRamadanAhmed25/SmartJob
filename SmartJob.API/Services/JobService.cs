using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class JobService : IJobService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IAIMatchService _aiMatch;

    public JobService(AppDbContext db, IMapper mapper, IAIMatchService aiMatch)
    {
        _db = db;
        _mapper = mapper;
        _aiMatch = aiMatch;
    }

    public async Task<PagedResult<JobDto>> GetJobsAsync(JobFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = _db.Jobs
            .Include(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(j => j.Applications)
            .Where(j => j.Status == JobStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(kw) ||
                j.Description.ToLower().Contains(kw) ||
                j.RequirementsJson.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var loc = filter.Location.ToLower();
            query = query.Where(j => j.Location.ToLower().Contains(loc));
        }

        if (!string.IsNullOrWhiteSpace(filter.Type) && Enum.TryParse<JobType>(filter.Type, true, out var jobType))
        {
            query = query.Where(j => j.Type == jobType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Salary))
        {
            var sal = filter.Salary.ToLower();
            query = query.Where(j => j.Salary.ToLower().Contains(sal));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var jobs = await query
            .OrderByDescending(j => j.PostedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<JobDto>
        {
            Items = _mapper.Map<List<JobDto>>(jobs),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<JobDto>> GetRecommendedJobsAsync(Guid seekerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var seeker = await _db.SeekerProfiles.FirstOrDefaultAsync(s => s.Id == seekerId, cancellationToken);
        if (seeker == null) return new PagedResult<JobDto> { Page = page, PageSize = pageSize };

        var seekerSkills = seeker.Skills.Select(s => s.ToLower()).ToHashSet();

        var jobs = await _db.Jobs
            .Include(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(j => j.Applications)
            .Where(j => j.Status == JobStatus.Active)
            .ToListAsync(cancellationToken);

        // Score jobs by skill overlap
        var scored = jobs
            .Select(j => new
            {
                Job = j,
                Score = j.Requirements.Count == 0 ? 0 :
                    j.Requirements.Count(r => seekerSkills.Contains(r.ToLower())) * 100 / j.Requirements.Count
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Job.PostedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<JobDto>
        {
            Items = _mapper.Map<List<JobDto>>(scored.Select(x => x.Job).ToList()),
            TotalCount = jobs.Count,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<JobDto> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs
            .Include(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        return _mapper.Map<JobDto>(job);
    }

    public async Task<JobDto> CreateJobAsync(Guid employerId, CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<JobType>(request.Type, true, out var jobType))
            jobType = JobType.FullTime;

        var job = new Job
        {
            EmployerId = employerId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Requirements = request.Requirements.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).ToList(),
            Location = request.Location.Trim(),
            Salary = request.Salary.Trim(),
            Type = jobType,
            ExpiresAt = request.ExpiresAt
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetJobByIdAsync(job.Id, cancellationToken);
    }

    public async Task<JobDto> UpdateJobAsync(Guid id, Guid employerId, UpdateJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        if (job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own this job.");

        if (request.Title != null) job.Title = request.Title.Trim();
        if (request.Description != null) job.Description = request.Description.Trim();
        if (request.Requirements != null) job.Requirements = request.Requirements.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).ToList();
        if (request.Location != null) job.Location = request.Location.Trim();
        if (request.Salary != null) job.Salary = request.Salary.Trim();
        if (request.Type != null && Enum.TryParse<JobType>(request.Type, true, out var jobType)) job.Type = jobType;
        if (request.ExpiresAt != null) job.ExpiresAt = request.ExpiresAt;

        await _db.SaveChangesAsync(cancellationToken);
        return await GetJobByIdAsync(id, cancellationToken);
    }

    public async Task PatchStatusAsync(Guid id, Guid employerId, string status, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        if (job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own this job.");

        if (!Enum.TryParse<JobStatus>(status, true, out var jobStatus))
            throw new ApiException(StatusCodes.Status400BadRequest, "Invalid status value.");

        job.Status = jobStatus;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteJobAsync(Guid id, Guid employerId, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        if (job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own this job.");

        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<AIAnalysisDto> GetAIAnalysisAsync(Guid jobId, Guid seekerId, CancellationToken cancellationToken = default)
        => _aiMatch.AnalyzeAsync(jobId, seekerId, cancellationToken);
}
