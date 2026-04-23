# Smart Job Recruitment App — Task Tracker

## Phase 1 — Core Foundation ✅ COMPLETE
> 🎯 Goal: Running API with auth, database, and user profiles

### 1.1 Project Setup
- [x] Create ASP.NET Core Web API solution (`SmartJob.API`)
- [x] Install NuGet packages:
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - `AutoMapper.Extensions.Microsoft.DependencyInjection`
  - `Swashbuckle.AspNetCore`
  - `Microsoft.AspNetCore.SignalR`
- [x] Configure `appsettings.json` (ConnectionStrings, JWT settings)
- [x] Set up folder structure: Controllers / Models / DTOs / Services / Repositories / Mappings / Hubs / Data

### 1.2 Database & Entities
- [x] Create `AppDbContext.cs` with DbSets for all 11 tables
- [x] Create entity models:
  - [x] `User.cs`
  - [x] `SeekerProfile.cs`
  - [x] `EmployerProfile.cs`
  - [x] `Resume.cs`
  - [x] `Job.cs`
  - [x] `Application.cs`
  - [x] `Interview.cs`
  - [x] `Chat.cs`
  - [x] `Message.cs`
  - [x] `Notification.cs`
  - [x] `Report.cs`
- [x] Add all enums: `UserRole`, `JobType`, `JobStatus`, `ApplicationStatus`, `InterviewMode`, `InterviewStatus`, `NotificationType`, `ReportType`
- [x] Run first EF Core migration: `InitialCreate`
- [x] Apply migration to Sqlite (DefaultConnection)
- [x] Transition to SQL Server (Production Ready)

### 1.3 Authentication Module
- [x] Create `AuthController.cs`
- [x] Implement `POST /api/auth/register` (Seeker & Employer)
- [x] Implement `POST /api/auth/login` → returns JWT + refresh token
- [x] Implement `POST /api/auth/refresh` → rotate tokens
- [x] Implement `POST /api/auth/logout` → invalidate refresh token
- [x] Implement `POST /api/auth/forgot-password`
- [x] Implement `POST /api/auth/reset-password`
- [x] Create `IAuthService` + `AuthService`
- [x] Create `AuthDto.cs` (LoginRequest, RegisterRequest, TokenResponse)
- [x] Create `AuthMappingProfile.cs`
- [x] Configure JWT middleware in `Program.cs`
- [x] Add role-based authorization policies

### 1.4 User & Profile Module
- [x] Create `UsersController.cs`
- [x] Implement `GET /api/users/me`
- [x] Implement `PUT /api/users/me` (update bio, name, phone)
- [x] Implement `PUT /api/users/me/avatar` (multipart upload)
- [x] Implement `PUT /api/users/me/seeker-profile` (skills, experience)
- [x] Implement `PUT /api/users/me/employer-profile` (company info)
- [x] Create `IUserService` + `UserService`
- [x] Create `UserDto.cs`, `SeekerProfileDto.cs`, `EmployerProfileDto.cs`
- [x] Create `UserMappingProfile.cs`

### 1.5 Resumes Module
- [x] Create `ResumesController.cs`
- [x] Implement `GET /api/resumes` (list my resumes)
- [x] Implement `POST /api/resumes` (multipart file upload → local disk)
- [x] Implement `DELETE /api/resumes/:id`
- [x] Implement `PATCH /api/resumes/:id/default`
- [x] Create `IResumeService` + `ResumeService`
- [x] Create `ResumeDto.cs`
- [x] Configure static file serving for uploads

### 1.6 Global Setup
- [x] Configure Swagger with JWT auth header support
- [x] Add global error handling middleware
- [x] Add request logging middleware
- [x] Test all Phase 1 endpoints with Swagger

---

## Phase 2 — Jobs & Applications
> 🎯 Goal: Full job posting + application flow + AI match score

### 2.1 Jobs Module
- [x] Create `JobsController.cs`
- [x] Implement `GET /api/jobs` (paginated, filters: keyword, location, type, salary)
- [x] Implement `GET /api/jobs/recommended` (AI-scored list for seeker)
- [x] Implement `GET /api/jobs/:id`
- [x] Implement `POST /api/jobs` (Employer only)
- [x] Implement `PUT /api/jobs/:id` (Employer only)
- [x] Implement `PATCH /api/jobs/:id/status`
- [x] Implement `DELETE /api/jobs/:id`
- [x] Create `IJobService` + `JobService`
- [x] Create `JobDto.cs`, `CreateJobRequest.cs`, `JobFilterRequest.cs`
- [x] Create `JobMappingProfile.cs`

### 2.2 Applications Module
- [x] Create `ApplicationsController.cs`
- [x] Implement `POST /api/applications` (Seeker applies)
- [x] Implement `GET /api/applications/my` (Seeker: my applications)
- [x] Implement `GET /api/applications/job/:jobId` (Employer: applicants list)
- [x] Implement `PATCH /api/applications/:id/status` (Employer: shortlist/reject)
- [x] Implement `GET /api/applications/:id`
- [x] Create `IApplicationService` + `ApplicationService`
- [x] Create `ApplicationDto.cs`, `ApplyRequest.cs`
- [x] Create `ApplicationMappingProfile.cs`
- [x] Auto-trigger notification on application received (Employer)
- [x] Auto-trigger notification on status change (Seeker)

### 2.3 AI Match Score
- [x] Create `IAIMatchService` + `AIMatchService`
- [x] Implement skill-overlap algorithm
- [x] Implement `GET /api/jobs/:id/ai-analysis` endpoint
- [x] Calculate + store `AIMatchScore` in `Applications` on apply

---

## Phase 3 — Interview Management
> 🎯 Goal: Full interview lifecycle + email notifications

### 3.1 Interviews Module
- [x] Create `InterviewsController.cs`
- [x] Implement `POST /api/interviews` (Employer schedules)
- [x] Implement `GET /api/interviews/my` (Seeker: my interviews)
- [x] Implement `GET /api/interviews/job/:jobId` (Employer: interviews for job)
- [x] Implement `PATCH /api/interviews/:id/accept`
- [x] Implement `PATCH /api/interviews/:id/reject`
- [x] Implement `POST /api/interviews/:id/reschedule`
- [x] Implement `PATCH /api/interviews/:id/complete`
- [x] Create `IInterviewService` + `InterviewService`
- [x] Create `InterviewDto.cs`, `ScheduleInterviewRequest.cs`
- [x] Create `InterviewMappingProfile.cs`

### 3.2 Email Notifications
- [x] Install `MailKit` or integrate `SendGrid`
- [x] Create `IEmailService` + `EmailService`
- [x] Send email on interview scheduled → Seeker
- [x] Send email on interview accepted/rejected/reschedule → Employer

---

## Phase 4 — Chat & Real-time
> 🎯 Goal: WhatsApp-style messaging + live notifications via SignalR

### 4.1 Chat REST Endpoints
- [x] Create `ChatsController.cs`
- [x] Implement `GET /api/chats` (my chat list with last message)
- [x] Implement `POST /api/chats` (create or get existing chat)
- [x] Implement `GET /api/chats/:id/messages` (paginated, newest first)
- [x] Implement `POST /api/chats/:id/messages` (send message via REST)
- [x] Implement `PATCH /api/chats/:id/read`
- [x] Create `IChatService` + `ChatService`
- [x] Create `ChatDto.cs`, `MessageDto.cs`, `SendMessageRequest.cs`
- [x] Create `ChatMappingProfile.cs`

### 4.2 SignalR — Chat Hub
- [x] Create `ChatHub.cs` in `/Hubs`
- [x] Implement `JoinChat(chatId)` — add to SignalR group
- [x] Implement `LeaveChat(chatId)`

### 4.3 SignalR — Notifications Hub
- [x] Create `NotificationsHub.cs`
- [x] Implement user mapping

### 4.4 Notifications REST Endpoints
- [x] Create `NotificationsController.cs`
- [x] Implement `GET /api/notifications`
- [x] Implement `PATCH /api/notifications/:id/read`
- [x] Implement `PATCH /api/notifications/read-all`
- [x] Implement `DELETE /api/notifications/:id`
- [x] Create `INotificationService` + `NotificationService`

---

## Phase 5 — Polish & Deployment ✅ COMPLETE
> 🎯 Goal: Production-ready API

### 5.1 Reports Module
- [x] Create `ReportsController.cs`
- [x] Implement `POST /api/reports`
- [x] Create `IReportService` + `ReportService`

### 5.2 API Documentation
- [x] Enable XML documentation in `.csproj`
- [x] Add Swagger JWT Auth configuration in `Program.cs`
- [x] Add `<summary>` XML comments to API controllers

### 5.3 Testing Setup ✅ COMPLETE
- [x] Create `SmartJob.Tests` xUnit project
- [x] Implement `AIMatchServiceTests`
- [x] Implement `AuthServiceTests`
- [x] Create simple integration flow test templates

### 5.4 Deployment
- [x] Add global CORS policy in `Program.cs`
- [x] Set up basic Rate Limiting middleware
- [x] Create `appsettings.Production.json`

## ✅ Project Status: FINALIZED
The Smart Job Backend is complete and ready for deployment.

