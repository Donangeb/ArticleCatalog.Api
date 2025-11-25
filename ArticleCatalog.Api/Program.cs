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

// Применяем миграции через middleware
app.UseMiddleware<ArticleCatalog.Infrastructure.Middleware.DatabaseMigrationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapControllers();

app.Run();
