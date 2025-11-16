using ArticleCatalog.Api.Extensions;
using ArticleCatalog.Infrastructure.Data;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ArticleCatalog.Application.Validators.CreateArticleRequestValidator>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddArticleCatalog(builder.Configuration);

var app = builder.Build();

// Применяем миграции при старте
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ArticleCatalogDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Applying database migrations...");
    
    var maxRetries = 3;
    var delay = TimeSpan.FromSeconds(3);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            // Проверяем, можем ли мы подключиться к базе данных
            if (!dbContext.Database.CanConnect())
            {
                logger.LogWarning("Cannot connect to database. Retrying in {Delay} seconds...", delay.TotalSeconds);
                Thread.Sleep(delay);
                continue;
            }
            
            // Проверяем, существует ли таблица миграций, и применяем миграции или создаем схему
            try
            {
                // Пытаемся проверить, существует ли таблица __EFMigrationsHistory
                var connection = dbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory');";
                var migrationsTableExists = (bool)command.ExecuteScalar()!;
                
                if (!migrationsTableExists)
                {
                    // Таблица миграций не существует, используем EnsureCreated
                    logger.LogWarning("Migrations history table does not exist. Using EnsureCreated to create database schema...");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Database schema created successfully using EnsureCreated.");
                }
                else
                {
                    // Таблица миграций существует, пытаемся применить миграции
                    var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Found {Count} pending migrations: {Migrations}", 
                            pendingMigrations.Count, string.Join(", ", pendingMigrations));
                        dbContext.Database.Migrate();
                        logger.LogInformation("Database migrations applied successfully.");
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations. Database is up to date.");
                    }
                }
            }
            catch (Exception migrationEx)
            {
                // Если что-то пошло не так, используем EnsureCreated как fallback
                logger.LogWarning(migrationEx, "Could not check/apply migrations. Using EnsureCreated to create database schema...");
                dbContext.Database.EnsureCreated();
                logger.LogInformation("Database schema created successfully using EnsureCreated fallback.");
            }
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Attempt {Attempt}/{MaxRetries} failed to migrate the database. Retrying in {Delay} seconds...", 
                i + 1, maxRetries, delay.TotalSeconds);
            
            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "Failed to apply database migrations after {MaxRetries} attempts.", maxRetries);
                // В последней попытке используем EnsureCreated как fallback
                try
                {
                    logger.LogWarning("Attempting to create database schema using EnsureCreated as fallback...");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Database schema created successfully using EnsureCreated fallback.");
                    break;
                }
                catch (Exception fallbackEx)
                {
                    logger.LogError(fallbackEx, "EnsureCreated also failed.");
                    throw;
                }
            }
            
            Thread.Sleep(delay);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
