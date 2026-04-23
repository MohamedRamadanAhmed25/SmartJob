using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartJob.API.Data;
using SmartJob.API.Mappings;
using SmartJob.API.Middleware;
using SmartJob.API.Options;
using SmartJob.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));

// Ensure basic options for tests or missing config
var fileStorageOptions = builder.Configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>() ?? new FileStorageOptions();
if (string.IsNullOrEmpty(fileStorageOptions.RootPath))
{
    fileStorageOptions.RootPath = "wwwroot/uploads";
    fileStorageOptions.AvatarFolder = "avatars";
    fileStorageOptions.ResumeFolder = "resumes";
    builder.Services.Configure<FileStorageOptions>(opts =>
    {
        opts.RootPath = fileStorageOptions.RootPath;
        opts.AvatarFolder = fileStorageOptions.AvatarFolder;
        opts.ResumeFolder = fileStorageOptions.ResumeFolder;
    });
}

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? new JwtOptions { Key = "TEST_LOCAL_KEY_SECURE_RANDOM_123456", Issuer = "Test", Audience = "Test" };

if (string.IsNullOrWhiteSpace(jwtOptions.Key)) 
{
    jwtOptions.Key = "TEST_LOCAL_KEY_SECURE_RANDOM_123456";
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
    {
        options.UseInMemoryDatabase("SmartJobIntegrationDb");
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString);
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ILocalFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IAIMatchService, AIMatchService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartJob API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SeekerOnly", policy => policy.RequireRole("Seeker"));
    options.AddPolicy("EmployerOnly", policy => policy.RequireRole("Employer"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // dbContext.Database.EnsureCreated();
    // Using migrations instead - Run 'dotnet ef database update' to apply.
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SmartJob.API.Hubs.ChatHub>("/hubs/chat");
app.MapHub<SmartJob.API.Hubs.NotificationsHub>("/hubs/notifications");

app.Run();

public partial class Program { }
