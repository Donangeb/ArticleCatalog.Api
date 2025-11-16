namespace ArticleCatalog.Application.DTOs;

public record ArticleDto (
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<string> Tags
);
