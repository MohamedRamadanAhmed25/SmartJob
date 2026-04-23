# Smart Job Recruitment App — Backend Implementation Plan

## Overview

The frontend (Figma Make) is a **React + Vite + TailwindCSS** mobile app with two user roles:
- **Job Seeker** — Browses jobs, applies, tracks status, manages interviews
- **Employer** — Posts jobs, reviews applicants (swipe UI), schedules interviews

The backend must support all data models and flows surfaced in the UI, including an **AI match score** system.

---

## Proposed Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ✅ ASP.NET Core Web API |
| **ORM** | ✅ Entity Framework Core |
| **Database** | ✅ SQL Server |
| **Auth** | JWT + Refresh Tokens (ASP.NET Identity) |
| **Real-time** | SignalR (built into ASP.NET Core) |
| **File Storage** | Local disk (Phase 1) → Azure Blob Storage (Phase 2) |
| **AI Matching** | Skill-overlap query (Phase 1) → OpenAI API (Phase 2) |
| **Email** | SendGrid / MailKit |
| **API Docs** | Swagger / Swashbuckle |

> [!NOTE]
> **Stack confirmed by user:** ASP.NET Core + EF Core + SQL Server. AutoMapper will be used for DTO mapping (consistent with your existing projects).

---

## Project Structure (ASP.NET Core)

```
SmartJob.API/
├── Controllers/          ← API Controllers per module
├── Models/               ← EF Core Entities
├── DTOs/                 ← Request/Response DTOs
├── Services/             ← Business logic interfaces + implementations
├── Repositories/         ← Data access layer
├── Mappings/             ← AutoMapper profiles
├── Hubs/                 ← SignalR hubs (Chat, Notifications)
├── Middleware/           ← Auth, error handling
├── Data/
│   ├── AppDbContext.cs   ← EF Core DbContext
│   └── Migrations/       ← EF Core migrations
└── Program.cs            ← DI setup, middleware pipeline
```

---

## Database Schema (SQL Server via EF Core)

### 1. `Users` Table
```
Id (uuid, PK)
Email (string, unique, required)
PasswordHash (string)
Role (enum: Seeker | Employer)
Name (string)
Phone (string, nullable)
Bio (string, nullable)
AvatarUrl (string, nullable)
IsVerified (bool)
CreatedAt (datetime)
UpdatedAt (datetime)
```

### 2. `SeekerProfiles` Table *(extends Users where Role = Seeker)*
```
Id (uuid, PK → Users.Id)
Skills (string[], JSON array)
ExperienceYears (int)
EducationLevel (enum)
LinkedInUrl (string, nullable)
GitHubUrl (string, nullable)
```

### 3. `EmployerProfiles` Table *(extends Users where Role = Employer)*
```
Id (uuid, PK → Users.Id)
CompanyName (string)
CompanySize (enum)
Industry (string)
CompanyLogoUrl (string, nullable)
Website (string, nullable)
```

### 4. `Resumes` Table
```
Id (uuid, PK)
SeekerId (uuid, FK → Users.Id)
FileName (string)
FileUrl (string)               ← Stored in S3/Blob
UploadedAt (datetime)
IsDefault (bool)
```

### 5. `Jobs` Table
```
Id (uuid, PK)
EmployerId (uuid, FK → Users.Id)
Title (string)
Description (text)
Requirements (string[], JSON)
Location (string)
Salary (string)
Type (enum: FullTime | PartTime | Contract | Remote)
Status (enum: Active | Paused | Closed)
PostedAt (datetime)
ExpiresAt (datetime, nullable)
```

### 6. `Applications` Table
```
Id (uuid, PK)
JobId (uuid, FK → Jobs.Id)
SeekerId (uuid, FK → Users.Id)
ResumeId (uuid, FK → Resumes.Id)
Status (enum: Sent | Viewed | Interview | Accepted | Rejected)
AIMatchScore (int, 0-100)
AppliedAt (datetime)
UpdatedAt (datetime)
```

### 7. `Interviews` Table
```
Id (uuid, PK)
ApplicationId (uuid, FK → Applications.Id)
ScheduledAt (datetime)
Mode (enum: Online | Onsite)
InterviewLink (string, nullable)    ← Used when Mode = Online
Status (enum: Pending | Accepted | Rejected | Completed)
RescheduleRequestedAt (datetime, nullable)
CreatedAt (datetime)
```

### 8. `Chats` Table
```
Id (uuid, PK)
JobId (uuid, FK → Jobs.Id)
SeekerId (uuid, FK → Users.Id)
EmployerId (uuid, FK → Users.Id)
JobTitle (string)                   ← Denormalized for sticky header
CreatedAt (datetime)
LastMessageAt (datetime)
```

### 9. `Messages` Table
```
Id (uuid, PK)
ChatId (uuid, FK → Chats.Id)
SenderId (uuid, FK → Users.Id)
Content (text)
SentAt (datetime)
IsRead (bool)
```

### 10. `Notifications` Table
```
Id (uuid, PK)
UserId (uuid, FK → Users.Id)
Type (enum: Interview | StatusChange | Message | NewApplicant)
Title (string)
Message (text)
IsRead (bool)
CreatedAt (datetime)
```

### 11. `Reports` Table *(Report Issue screen)*
```
Id (uuid, PK)
ReporterId (uuid, FK → Users.Id)
ReportType (enum: Bug | Inappropriate | Spam | Other)
Description (text)
Status (enum: Open | InProgress | Resolved)
CreatedAt (datetime)
```

---

## API Endpoints

### 🔐 Auth Module — `/api/auth`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/register` | Register (role: seeker/employer) |
| POST | `/login` | Login → returns JWT + refreshToken |
| POST | `/refresh` | Refresh JWT |
| POST | `/logout` | Invalidate token |
| POST | `/forgot-password` | Send reset email |
| POST | `/reset-password` | Reset with token |
| GET | `/verify-email/:token` | Email verification |

### 👤 User / Profile Module — `/api/users`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/me` | Get current user profile |
| PUT | `/me` | Update profile (bio, name, phone) |
| PUT | `/me/avatar` | Upload avatar (multipart) |
| PUT | `/me/seeker-profile` | Update seeker skills/experience |
| PUT | `/me/employer-profile` | Update company info |

### 📄 Resumes Module — `/api/resumes`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | List my resumes |
| POST | `/` | Upload resume (multipart/form-data) |
| DELETE | `/:id` | Delete a resume |
| PATCH | `/:id/default` | Set as default resume |

### 💼 Jobs Module — `/api/jobs`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | List jobs (filters: location, type, salary, keyword) |
| GET | `/recommended` | AI-recommended jobs for seeker |
| GET | `/:id` | Get job details |
| POST | `/` | Create job (Employer only) |
| PUT | `/:id` | Update job (Employer only) |
| PATCH | `/:id/status` | Change status: active/paused/closed |
| DELETE | `/:id` | Delete job |
| GET | `/:id/ai-analysis` | AI match score + breakdown for current user |

### 📬 Applications Module — `/api/applications`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/` | Apply to job `{jobId, resumeId}` |
| GET | `/my` | Seeker: list my applications |
| GET | `/job/:jobId` | Employer: list applicants for a job |
| PATCH | `/:id/status` | Employer: update application status |
| GET | `/:id` | Get single application details |

### 📅 Interviews Module — `/api/interviews`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/` | Schedule interview `{applicationId, scheduledAt, mode, link?}` |
| GET | `/my` | Seeker: list my interviews |
| GET | `/job/:jobId` | Employer: list interviews for a job |
| PATCH | `/:id/accept` | Seeker accepts interview |
| PATCH | `/:id/reject` | Seeker rejects interview |
| POST | `/:id/reschedule` | Seeker requests reschedule |
| PATCH | `/:id/complete` | Employer marks as completed |

### 💬 Chat Module — `/api/chats`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | List my chats |
| GET | `/:chatId/messages` | Get messages for a chat (paginated) |
| POST | `/` | Create/get chat `{jobId, participantId}` |
| POST | `/:chatId/messages` | Send a message (also via WebSocket) |
| PATCH | `/:chatId/read` | Mark all messages as read |

### 🔔 Notifications Module — `/api/notifications`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | List my notifications |
| PATCH | `/:id/read` | Mark as read |
| PATCH | `/read-all` | Mark all as read |
| DELETE | `/:id` | Delete notification |

### 🚨 Reports Module — `/api/reports`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/` | Submit a report `{reportType, description}` |

---

## AI Match Score Logic

The AI match badge (e.g., "85% AI Match") is computed as:

```
MatchScore = weighted_sum(
  skill_overlap_score    × 0.50,   // Seeker.Skills ∩ Job.Requirements
  experience_score       × 0.20,   // SeekerProfile.ExperienceYears vs job level
  location_score         × 0.15,   // Location match or remote
  application_behavior   × 0.15    // Past acceptance rate, activity
)
```

**Implementation Options:**
1. **Simple (Phase 1):** SQL Server JSON field queries — compare `SeekerProfile.Skills` array against `Job.Requirements` array
2. **Advanced (Phase 2):** OpenAI API integration for semantic skill matching (embeddings)

The endpoint `GET /api/jobs/:id/ai-analysis` returns:
```json
{
  "matchScore": 85,
  "matchingSkills": ["React", "TypeScript"],
  "missingSkills": ["GraphQL"],
  "whyMatch": ["5+ years experience", "Strong TypeScript background"]
}
```

---

## Real-time Features (SignalR / WebSocket)

### Hub: `/hubs/chat`
- `JoinChat(chatId)` — Subscribe to chat room
- `SendMessage(chatId, content)` — Send message + broadcast
- `ReceiveMessage` — Client event

### Hub: `/hubs/notifications`
- Auto-triggered on:
  - Application status change → notify Seeker
  - New applicant → notify Employer
  - Interview scheduled/accepted/rejected
  - New message received

---

## Authentication Strategy

```
POST /api/auth/login
→ Returns:
  - accessToken (JWT, 15min expiry)
  - refreshToken (opaque, stored in DB, 7 days)

All protected routes require:
Authorization: Bearer <accessToken>

Role-based Authorization:
  [Authorize(Roles = "Seeker")]   → Seeker-only endpoints
  [Authorize(Roles = "Employer")] → Employer-only endpoints
```

---

## Execution Phases

### Phase 1 — Core Foundation (Week 1-2) ✅ COMPLETED
- [x] Project setup (ASP.NET Core Web API)
- [x] SQL Server database + EF Core migrations
- [x] All 11 entity classes + DbContext
- [x] Auth module (register, login, JWT, refresh token)
- [x] User/Profile CRUD endpoints
- [x] Resume upload (local disk storage)

### Phase 2 — Job & Application Flow (Week 3-4) ✅ COMPLETED
- [x] Jobs CRUD (employer: post/edit/delete)
- [x] Job listing + search + filters (seeker)
- [x] Application submission + status tracking
- [x] Simple AI match score (skill overlap via SQL Server JSON queries)
- [x] Application status timeline data

### Phase 3 — Interview Management (Week 5) ✅ COMPLETED
- [x] Interview scheduling (employer)
- [x] Interview accept/reject/reschedule (seeker)
- [x] Interview notifications via email (MailKit)

### Phase 4 — Communication (Week 6) ✅ COMPLETED
- [x] Chat rooms (Chats + Messages tables)
- [x] REST chat endpoints
- [x] SignalR real-time messaging
- [x] Push notifications system

### Phase 5 — Polish & Deployment (Current Focus) 🔄 IN PROGRESS
- [x] Reports module
- [ ] Swagger/OpenAPI Documentation & JWT Config
- [ ] Unit & Integration Tests (xUnit, Moq)
- [ ] Advanced AI matching (embeddings/OpenAI) - *Optional*
- [ ] Deployment Configuration (CORS, Rate Limiting, `appsettings.Production.json`)

---

## User Review Required

> [!IMPORTANT]
> **Swagger JWT Auth Missing:** I noticed that the Swagger configuration in `Program.cs` currently lacks the `AddSecurityDefinition` for JWT Bearer tokens, even though it was checked off in Phase 1. I will add this to make the API testable via Swagger.

## Proposed Changes

### Tests
#### [NEW] [SmartJob.Tests/](file:///c:/Users/ELSAKA/Downloads/Smart-Job-main/Smart-Job-main/SmartJob.Tests)
Create an xUnit test project referencing `SmartJob.API`.
- **Unit Tests:** `AIMatchServiceTests`, `AuthServiceTests`
- **Integration Tests:** Endpoint tests using `WebApplicationFactory` and an InMemory database to verify the Jobs & Applications flow.

### API Documentation
#### [MODIFY] [SmartJob.API.csproj](file:///c:/Users/ELSAKA/Downloads/Smart-Job-main/Smart-Job-main/SmartJob.API/SmartJob.API.csproj)
Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and suppress warnings for missing XML comments on non-essential elements (`<NoWarn>$(NoWarn);1591</NoWarn>`).
#### [MODIFY] [Program.cs](file:///c:/Users/ELSAKA/Downloads/Smart-Job-main/Smart-Job-main/SmartJob.API/Program.cs)
Configure Swagger to use XML comments (`c.IncludeXmlComments(...)`) and add Bearer token authentication support so the UI can be tested.
#### [MODIFY] [Controllers/*.cs](file:///c:/Users/ELSAKA/Downloads/Smart-Job-main/Smart-Job-main/SmartJob.API/Controllers)
Add `<summary>` XML comments to all public endpoints across all controllers, outlining what each endpoint does and any specific requirements.

### Deployment Configuration
#### [MODIFY] [Program.cs](file:///c:/Users/ELSAKA/Downloads/Smart-Job-main/Smart-Job-main/SmartJob.API/Program.cs)
Add simple global CORS policy (AllowAnyOrigin/Method/Header - restricted appropriately if requested) and Rate Limiting middleware.
#### [NEW] [appsettings.Production.json](file:///c:/Users/ELSAKA/Downloads/Smart-Job-main/Smart-Job-main/SmartJob.API/appsettings.Production.json)
Create a production configuration file template (ready for environment variables).

---

## Open Questions

> [!WARNING]
> **1. Advanced AI Match:** The current `AIMatchService` relies on a pure skill overlap algorithm. Should we integrate **OpenAI embeddings** to compute match scores, or keep the simple algorithm and consider it feature-complete?

> [!WARNING]
> **2. Testing Scope:** Can I focus on writing complete unit tests for the complex services (`AIMatchService` and `AuthService`) and a few critical integration endpoints to ensure the project correctly builds and tests?

> [!IMPORTANT]
> **3. Approval:** Does this plan align with your vision of "completing" the project? If you approve, I will begin implementing Swagger XML docs, Tests, and deployment setup.
