namespace ArticleCatalog.Application.DTOs;

public record UpdateArticleRequest(
    string Title,
    List<string> Tags   
);