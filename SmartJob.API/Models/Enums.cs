namespace SmartJob.API.Models;

public enum UserRole { Seeker, Employer }

public enum JobType { FullTime, PartTime, Contract, Remote }

public enum JobStatus { Active, Paused, Closed }

public enum ApplicationStatus { Sent, Viewed, Interview, Accepted, Rejected }

public enum InterviewMode { Online, Onsite }

public enum InterviewStatus { Pending, Accepted, Rejected, Completed }

public enum NotificationType { Interview, StatusChange, Message, NewApplicant }

public enum ReportType { Bug, Inappropriate, Spam, Other }

public enum ReportStatus { Open, InProgress, Resolved }

public enum EducationLevel { HighSchool, Associate, Bachelor, Master, PhD, Other }

public enum CompanySize { Startup, Small, Medium, Large, Enterprise }
