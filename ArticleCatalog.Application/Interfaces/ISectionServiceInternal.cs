namespace ArticleCatalog.Application.Interface;

public interface ISectionServiceInternal
{
    Task AssignArticleToSectionAsync(Guid articleId);
    Task CleanupSectionsAsync();
}
