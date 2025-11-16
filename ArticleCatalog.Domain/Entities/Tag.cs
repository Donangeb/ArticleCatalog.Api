namespace ArticleCatalog.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
        public ICollection<SectionTag> SectionTags { get; set; } = new List<SectionTag>();
    }
}
