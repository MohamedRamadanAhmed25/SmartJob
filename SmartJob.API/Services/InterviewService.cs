using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Interviews;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class InterviewService : IInterviewService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public InterviewService(AppDbContext db, IMapper mapper, IEmailService emailService)
    {
        _db = db;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<InterviewDto> ScheduleInterviewAsync(Guid employerId, ScheduleInterviewRequest request, CancellationToken cancellationToken = default)
    {
        var app = await _db.Applications
            .Include(a => a.Job)
            .Include(a => a.Seeker)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Application not found.");

        if (app.Job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own this job.");

        var existing = await _db.Interviews.FirstOrDefaultAsync(i => i.ApplicationId == app.Id, cancellationToken);
        if (existing != null)
            throw new ApiException(StatusCodes.Status400BadRequest, "An interview is already associated with this application.");

        if (!Enum.TryParse<InterviewMode>(request.Mode, true, out var mode))
            throw new ApiException(StatusCodes.Status400BadRequest, "Invalid interview mode.");

        var interview = new Interview
        {
            ApplicationId = app.Id,
            ScheduledAt = request.ScheduledAt,
            Mode = mode,
            InterviewLink = request.InterviewLink,
            Status = InterviewStatus.Pending
        };

        _db.Interviews.Add(interview);
        
        app.Status = ApplicationStatus.Interview;
        app.UpdatedAt = DateTime.UtcNow;

        var notification = new Notification
        {
            UserId = app.SeekerId,
            Type = NotificationType.Interview,
            Title = "Interview Scheduled",
            Message = $"You have been invited for an interview for {app.Job.Title} on {interview.ScheduledAt:g}.",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);

        await _db.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailAsync(
            app.Seeker.Email,
            "Interview Scheduled",
            $"<p>You have an interview scheduled for the position of <b>{app.Job.Title}</b> on {interview.ScheduledAt:f}.</p>",
            cancellationToken);

        return await GetInterviewDtoAsync(interview.Id, cancellationToken);
    }

    public async Task<PagedResult<InterviewDto>> GetMyInterviewsAsync(Guid seekerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(i => i.Application).ThenInclude(a => a.Seeker)
            .Where(i => i.Application.SeekerId == seekerId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        
        var interviews = await query
            .OrderBy(i => i.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InterviewDto>
        {
            Items = _mapper.Map<List<InterviewDto>>(interviews),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<InterviewDto>> GetJobInterviewsAsync(Guid jobId, Guid employerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        if (job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own this job.");

        var query = _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(i => i.Application).ThenInclude(a => a.Seeker)
            .Where(i => i.Application.JobId == jobId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        
        var interviews = await query
            .OrderBy(i => i.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InterviewDto>
        {
            Items = _mapper.Map<List<InterviewDto>>(interviews),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InterviewDto> AcceptInterviewAsync(Guid id, Guid seekerId, CancellationToken cancellationToken = default)
    {
        var interview = await _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Employer)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Interview not found.");

        if (interview.Application.SeekerId != seekerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "This interview is not yours.");

        interview.Status = InterviewStatus.Accepted;

        var notification = new Notification
        {
            UserId = interview.Application.Job.EmployerId,
            Type = NotificationType.Interview,
            Title = "Interview Accepted",
            Message = $"The candidate accepted the interview for {interview.Application.Job.Title}.",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);

        await _db.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailAsync(
            interview.Application.Job.Employer.Email,
            "Interview Accepted",
            $"<p>The candidate accepted the interview for {interview.Application.Job.Title} scheduled at {interview.ScheduledAt:f}.</p>",
            cancellationToken);

        return await GetInterviewDtoAsync(id, cancellationToken);
    }

    public async Task<InterviewDto> RejectInterviewAsync(Guid id, Guid seekerId, CancellationToken cancellationToken = default)
    {
        var interview = await _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Employer)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Interview not found.");

        if (interview.Application.SeekerId != seekerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "This interview is not yours.");

        interview.Status = InterviewStatus.Rejected;

        var notification = new Notification
        {
            UserId = interview.Application.Job.EmployerId,
            Type = NotificationType.Interview,
            Title = "Interview Rejected",
            Message = $"The candidate rejected the interview for {interview.Application.Job.Title}.",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);

        await _db.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailAsync(
            interview.Application.Job.Employer.Email,
            "Interview Rejected",
            $"<p>The candidate rejected the interview for {interview.Application.Job.Title}.</p>",
            cancellationToken);

        return await GetInterviewDtoAsync(id, cancellationToken);
    }

    public async Task<InterviewDto> RescheduleInterviewAsync(Guid id, Guid seekerId, RescheduleRequest request, CancellationToken cancellationToken = default)
    {
        var interview = await _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Employer)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Interview not found.");

        if (interview.Application.SeekerId != seekerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "This interview is not yours.");

        interview.RescheduleRequestedAt = request.ProposedAt;

        var notification = new Notification
        {
            UserId = interview.Application.Job.EmployerId,
            Type = NotificationType.Interview,
            Title = "Interview Reschedule Requested",
            Message = $"The candidate requested to reschedule the interview for {interview.Application.Job.Title} to {request.ProposedAt:g}.",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);

        await _db.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailAsync(
            interview.Application.Job.Employer.Email,
            "Interview Reschedule Requested",
            $"<p>The candidate requested to reschedule the interview for {interview.Application.Job.Title} to {request.ProposedAt:f}.</p>",
            cancellationToken);

        return await GetInterviewDtoAsync(id, cancellationToken);
    }

    public async Task<InterviewDto> CompleteInterviewAsync(Guid id, Guid employerId, CancellationToken cancellationToken = default)
    {
        var interview = await _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Job)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Interview not found.");

        if (interview.Application.Job.EmployerId != employerId)
            throw new ApiException(StatusCodes.Status403Forbidden, "You do not own this job.");

        interview.Status = InterviewStatus.Completed;
        await _db.SaveChangesAsync(cancellationToken);

        return await GetInterviewDtoAsync(id, cancellationToken);
    }

    private async Task<InterviewDto> GetInterviewDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        var interview = await _db.Interviews
            .Include(i => i.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(i => i.Application).ThenInclude(a => a.Seeker)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            
        return _mapper.Map<InterviewDto>(interview)!;
    }
}
