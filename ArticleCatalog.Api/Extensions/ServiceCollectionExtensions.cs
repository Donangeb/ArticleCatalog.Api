using ArticleCatalog.Application.EventHandlers;
using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Application.Services;
using ArticleCatalog.Application.Validators;
using ArticleCatalog.Domain.DomainServices;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Infrastructure.Data;
using ArticleCatalog.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ArticleCatalog.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArticleCatalog(
        this IServiceCollection services,
        IConfiguration config)
    {
        // DbContext
        services.AddDbContext<ArticleCatalogDbContext>(options =>
        {
            options.UseNpgsql(
                config.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ArticleCatalogDbContext).Assembly.GetName().Name));
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories (Domain interfaces, Infrastructure implementations)
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Domain Services
        services.AddScoped<ISectionGenerator, Domain.DomainServices.SectionGenerator>();

        // Application Services
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<ISectionServiceInternal, SectionService>();

        // Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Domain Event Handlers
        services.AddScoped<IDomainEventHandler<Domain.Events.ArticleCreatedEvent>, ArticleCreatedEventHandler>();
        services.AddScoped<IDomainEventHandler<Domain.Events.ArticleTagsChangedEvent>, ArticleTagsChangedEventHandler>();
        services.AddScoped<IDomainEventHandler<Domain.Events.ArticleDeletedEvent>, ArticleDeletedEventHandler>();

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateArticleRequestValidator>();

        return services;
    }
}



