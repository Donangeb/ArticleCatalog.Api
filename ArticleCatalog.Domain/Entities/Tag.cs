namespace ArticleCatalog.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; private set; } = string.Empty;

        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
        public ICollection<SectionTag> SectionTags { get; set; } = new List<SectionTag>();

        /// <summary>
        /// Устанавливает имя тега с автоматической нормализацией
        /// </summary>
        public void SetName(string name)
        {
            Name = name.Trim();
            NormalizedName = name.Trim().ToLowerInvariant();
        }
    }
}
