namespace ArticleCatalog.Application.DTOs
{
    public record SectionDto(
        Guid Id,
        string Title,
        int TagCount,
        long ArticlesCount,
        IReadOnlyList<string> Tags
    );
}
