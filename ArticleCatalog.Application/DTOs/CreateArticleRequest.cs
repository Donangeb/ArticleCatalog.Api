namespace ArticleCatalog.Application.DTOs;

public record CreateArticleRequest(
    string Title,
    List<string> Tags
);
