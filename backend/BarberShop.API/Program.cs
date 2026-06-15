using BarberShop.API.Configuration;
using BarberShop.API.Data;
using BarberShop.API.Entities;
using BarberShop.API.HealthChecks;
using BarberShop.API.Middleware;
using BarberShop.API.Services.Appointments;
using BarberShop.API.Services.Appointments.Booking;
using BarberShop.API.Services.Auth;
using BarberShop.API.Services.Client;
using BarberShop.API.Services.Email;
using BarberShop.API.Services.EmployeePortal;
using BarberShop.API.Services.Employees;
using BarberShop.API.Services.Inventory;
using BarberShop.API.Services.Notifications;
using BarberShop.API.Services.Notifications.Factories;
using BarberShop.API.Services.Notifications.Strategies;
using BarberShop.API.Services.Public;
using BarberShop.API.Services.Owner;
using BarberShop.API.Services.ServiceCategories;
using BarberShop.API.Services.Security;
using BarberShop.API.Services.Services;
using BarberShop.API.Services.Shops;
using BarberShop.API.Services.Reviews;
using BarberShop.API.Serialization;
using BarberShop.API.Services.TimeOff;
using BarberShop.API.Services.WorkingHours;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BarberShop API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token in this format: Bearer {JWT_TOKEN}"
    });

    options.OperationFilter<SwaggerAuthorizeOperationFilter>();
});

builder.Services.AddDbContext<BarberShopDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHealthChecks()
    .AddCheck("api", () => HealthCheckResult.Healthy("API is healthy."))
    .AddCheck<DatabaseHealthCheck>("database");

var jwtIssuer = GetRequiredConfigurationValue(builder.Configuration, "Jwt:Issuer");
var jwtAudience = GetRequiredConfigurationValue(builder.Configuration, "Jwt:Audience");
var jwtKey = GetRequiredConfigurationValue(builder.Configuration, "Jwt:Key");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.Configure<CorsSettings>(
    builder.Configuration.GetSection("CorsSettings"));
var corsSettings = builder.Configuration
    .GetSection("CorsSettings")
    .Get<CorsSettings>() ?? new CorsSettings();
var allowedCorsOrigins = corsSettings.AllowedOrigins
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicies.FrontendPolicy, policy =>
    {
        policy.WithOrigins(allowedCorsOrigins)
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .WithHeaders("Authorization", "Content-Type");
    });
});
builder.Services.Configure<AuthSettings>(
    builder.Configuration.GetSection("AuthSettings"));
builder.Services.Configure<AuthRateLimitSettings>(
    builder.Configuration.GetSection("AuthRateLimitSettings"));
builder.Services.Configure<AccountLockoutSettings>(
    builder.Configuration.GetSection("AccountLockoutSettings"));
builder.Services.Configure<PasswordResetTokenCleanupSettings>(
    builder.Configuration.GetSection("PasswordResetTokenCleanupSettings"));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
var authRateLimitSettings = builder.Configuration
    .GetSection("AuthRateLimitSettings")
    .Get<AuthRateLimitSettings>() ?? new AuthRateLimitSettings();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AuthRateLimitPolicies.Login, httpContext =>
        CreateAuthRateLimitPartition(httpContext, authRateLimitSettings.Login));
    options.AddPolicy(AuthRateLimitPolicies.Register, httpContext =>
        CreateAuthRateLimitPartition(httpContext, authRateLimitSettings.Register));
    options.AddPolicy(AuthRateLimitPolicies.ForgotPassword, httpContext =>
        CreateAuthRateLimitPartition(httpContext, authRateLimitSettings.ForgotPassword));
    options.AddPolicy(AuthRateLimitPolicies.ResetPassword, httpContext =>
        CreateAuthRateLimitPartition(httpContext, authRateLimitSettings.ResetPassword));
    options.AddPolicy(AuthRateLimitPolicies.AppointmentBooking, httpContext =>
        CreateUserOrIpRateLimitPartition(httpContext, authRateLimitSettings.AppointmentBooking));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientPortalService, ClientPortalService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IOwnerAccessService, OwnerAccessService>();
builder.Services.AddScoped<IEmployeeAccessService, EmployeeAccessService>();
builder.Services.AddScoped<IEmployeePortalService, EmployeePortalService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<ITimeOffService, TimeOffService>();
builder.Services.AddScoped<IWorkingHourService, WorkingHourService>();
builder.Services.AddScoped<IAppointmentBookingFacade, AppointmentBookingFacade>();
builder.Services.AddScoped<IAppointmentManagementService, AppointmentManagementService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IInventoryItemService, InventoryItemService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPublicShopService, PublicShopService>();
builder.Services.AddScoped<IOwnerPortalService, OwnerPortalService>();
builder.Services.AddScoped<INotificationStrategyFactory, NotificationStrategyFactory>();
builder.Services.AddScoped<INotificationStrategy, AppointmentBookedNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, AppointmentConfirmedNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, AppointmentCancelledNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, AppointmentCompletedNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, AppointmentNoShowNotificationStrategy>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddHostedService<PasswordResetTokenCleanupService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Use(async (context, next) =>
{
    var isWorkingHoursWrite =
        context.Request.Path.StartsWithSegments("/api/working-hours", StringComparison.OrdinalIgnoreCase) &&
        (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method));

    if (!app.Environment.IsDevelopment() || !isWorkingHoursWrite)
    {
        await next();
        return;
    }

    context.Request.EnableBuffering();

    using var reader = new StreamReader(
        context.Request.Body,
        Encoding.UTF8,
        detectEncodingFromByteOrderMarks: false,
        leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;

    app.Logger.LogInformation("Working hours request body: {Body}", body);

    await next();
});

app.UseHttpsRedirection();

app.UseCors(CorsPolicies.FrontendPolicy);

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponseAsync
}).AllowAnonymous();

app.MapControllers();

app.Run();

static string GetRequiredConfigurationValue(IConfiguration configuration, string key)
{
    var value = configuration[key];
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"{key} is required and must not be empty.");
    }

    return value;
}

static RateLimitPartition<string> CreateAuthRateLimitPartition(
    HttpContext httpContext,
    EndpointRateLimitSettings settings)
{
    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    return RateLimitPartition.GetFixedWindowLimiter(
        clientIp,
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit > 0 ? settings.PermitLimit : 1,
            Window = TimeSpan.FromSeconds(settings.WindowSeconds > 0 ? settings.WindowSeconds : 60),
            QueueLimit = settings.QueueLimit >= 0 ? settings.QueueLimit : 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
}

static RateLimitPartition<string> CreateUserOrIpRateLimitPartition(
    HttpContext httpContext,
    EndpointRateLimitSettings settings)
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var partitionKey = !string.IsNullOrWhiteSpace(userId)
        ? $"user:{userId}"
        : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey,
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit > 0 ? settings.PermitLimit : 1,
            Window = TimeSpan.FromSeconds(settings.WindowSeconds > 0 ? settings.WindowSeconds : 60),
            QueueLimit = settings.QueueLimit >= 0 ? settings.QueueLimit : 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
}

static async Task WriteHealthCheckResponseAsync(HttpContext httpContext, HealthReport healthReport)
{
    httpContext.Response.ContentType = "application/json";

    var response = new
    {
        status = healthReport.Status.ToString(),
        checks = healthReport.Entries.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Status.ToString())
    };

    await httpContext.Response.WriteAsJsonAsync(response);
}
