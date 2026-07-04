using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
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
using SmartJob.API.AI.Services;
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

builder.Services.AddScoped<IAiService, AiService>();

builder.Services.AddScoped<IAIMatchService, AIMatchService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // Fix for Circular References
        opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartJob API", Version = "v1" });

    // 1. ????? ??? ???? ????? ?????? (???????)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 2. ????? ??????? ?????? ??? ???? ????????
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

    // ????? ???? ????? ??? XML ????????? (???????)
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
        // Allow all origins for REST endpoints
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // For SignalR: must use specific origins if AllowCredentials is needed
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200",
                "http://localhost:8080",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
    //    // dbContext.Database.EnsureCreated();
    //    // Using migrations instead - Run 'dotnet ef database update' to apply.
    //dbContext.Database.Migrate();
}


// Ensure wwwroot/uploads directories exist
var uploadsRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
Directory.CreateDirectory(Path.Combine(uploadsRoot, "avatars"));
Directory.CreateDirectory(Path.Combine(uploadsRoot, "resumes"));



//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
//{
app.UseSwagger();
app.UseSwaggerUI();
app.Environment.IsDevelopment();
//}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();
app.MapHub<SmartJob.API.Hubs.ChatHub>("/hubs/chat").RequireCors("SignalRPolicy");
app.MapHub<SmartJob.API.Hubs.NotificationsHub>("/hubs/notifications").RequireCors("SignalRPolicy");

app.Run();
//app.UseHttpsRedirection();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
public partial class Program { }