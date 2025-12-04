using LudusLogicae.Api.Core.Configuration;
using LudusLogicae.Api.Core.Extensions;
using LudusLogicae.Api.Core.Middleware;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers()
    .AddApiJsonOptions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationOptions(configuration);

builder.Services.AddDatabase(configuration);

builder.Services.AddLlmServices();

builder.Services.AddApplicationServices();

builder.Services.AddApplicationHealthChecks(configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

await ApplyMigrationsAsync(app);

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var healthCheckOptions = app.Services.GetRequiredService<IOptions<HealthCheckOptions>>().Value;
app.MapHealthChecks(healthCheckOptions.Path);

app.Run();

async Task ApplyMigrationsAsync(WebApplication application)
{
    var logger = application.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var databaseOptions = application.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

    if (!databaseOptions.AutoMigrate)
    {
        logger.LogInformation("Database auto-migration is disabled");
        return;
    }

    var startTime = DateTime.UtcNow;
    var deadline = startTime.AddSeconds(Math.Max(0, databaseOptions.MigrationRetrySeconds));
    var attemptNumber = 0;

    while (true)
    {
        attemptNumber++;
        try
        {
            using var scope = application.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully on attempt {Attempt}", attemptNumber);
            return;
        }
        catch (Exception ex)
        {
            if (DateTime.UtcNow >= deadline)
            {
                logger.LogError(
                    ex,
                    "Failed to apply database migrations within retry window ({Seconds}s). Starting without applying migrations.",
                    databaseOptions.MigrationRetrySeconds);
                return;
            }

            var delayMs = CalculateExponentialBackoff(
                attemptNumber,
                databaseOptions.RetryBaseDelayMs,
                databaseOptions.RetryMaxDelayMs);
            logger.LogWarning(
                ex,
                "Database migration attempt {Attempt} failed. Retrying in {DelayMs}ms...",
                attemptNumber,
                delayMs);

            await Task.Delay(delayMs);
        }
    }
}

static int CalculateExponentialBackoff(int attempt, int baseDelay, int maxDelay)
{
    return (int)Math.Min(maxDelay, baseDelay * Math.Pow(2, attempt - 1));
}
