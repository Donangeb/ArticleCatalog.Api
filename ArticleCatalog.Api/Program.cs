using ArticleCatalog.Api.Extensions;
using ArticleCatalog.Application.Validators;
using ArticleCatalog.Infrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Автоматическая серверная валидация
builder.Services.AddFluentValidationAutoValidation();

// Регистрация валидаторов
builder.Services.AddValidatorsFromAssemblyContaining<CreateArticleRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddArticleCatalog(builder.Configuration);

var app = builder.Build();

// Применяем миграции при старте
using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ArticleCatalogDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var maxRetries = 3;
    var delay = TimeSpan.FromSeconds(3);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            if (!await dbContext.Database.CanConnectAsync())
            {
                logger.LogWarning("Cannot connect to database. Retry {Attempt}/{MaxRetries}...",
                    attempt, maxRetries);

                await Task.Delay(delay);
                continue;
            }

            await DbInitializer.ApplyMigrationsAsync(dbContext, logger);
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt}/{MaxRetries} failed.", attempt, maxRetries);

            if (attempt == maxRetries)
            {
                logger.LogError(ex, "All attempts failed. Using EnsureCreated fallback.");
                await dbContext.Database.EnsureCreatedAsync();
            }

            await Task.Delay(delay);
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
