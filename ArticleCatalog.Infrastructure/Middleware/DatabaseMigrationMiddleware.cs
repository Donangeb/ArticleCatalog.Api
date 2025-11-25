using ArticleCatalog.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Infrastructure.Middleware;

/// <summary>
/// Middleware для применения миграций базы данных при старте приложения
/// </summary>
public class DatabaseMigrationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseMigrationMiddleware> _logger;
    private static bool _migrationsApplied = false;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public DatabaseMigrationMiddleware(RequestDelegate next, ILogger<DatabaseMigrationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        // Применяем миграции только один раз при первом запросе
        if (!_migrationsApplied)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!_migrationsApplied)
                {
                    await ApplyMigrationsAsync(serviceProvider);
                    _migrationsApplied = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await _next(context);
    }

    private async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ArticleCatalogDbContext>();

        var maxRetries = 3;
        var delay = TimeSpan.FromSeconds(3);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (!await dbContext.Database.CanConnectAsync())
                {
                    _logger.LogWarning("Cannot connect to database. Retry {Attempt}/{MaxRetries}...",
                        attempt, maxRetries);

                    await Task.Delay(delay);
                    continue;
                }

                await DbInitializer.ApplyMigrationsAsync(dbContext, _logger);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Migration attempt {Attempt}/{MaxRetries} failed.", attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, "All attempts failed. Using EnsureCreated fallback.");
                    await dbContext.Database.EnsureCreatedAsync();
                }

                await Task.Delay(delay);
            }
        }
    }
}

