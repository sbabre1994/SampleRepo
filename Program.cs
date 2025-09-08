using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using UpgradeNotificationSystem.Data;
using UpgradeNotificationSystem.Hubs;
using UpgradeNotificationSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=upgrade_notification;Username=postgres;Password=postgres;";
builder.Services.AddDbContext<UpgradeContext>(options =>
    options.UseNpgsql(connectionString));

// SignalR
builder.Services.AddSignalR();

// Hangfire with PostgreSQL
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

// AWS S3 (use mock service in development)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IS3SyncService, MockS3SyncService>();
}
else
{
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonS3>();
    builder.Services.AddScoped<IS3SyncService, S3SyncService>();
}

// Application services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUpgradeManagementService, UpgradeManagementService>();
builder.Services.AddScoped<IUpgradeJobService, UpgradeJobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ReactApp");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<UpgradeNotificationHub>("/upgradeHub");

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UpgradeContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not ensure database is created. Make sure PostgreSQL is running.");
    }
}

app.Run();
