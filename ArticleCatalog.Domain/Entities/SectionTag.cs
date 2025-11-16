namespace ArticleCatalog.Domain.Entities
{
    public class SectionTag
    {
        public Guid SectionId { get; set; }
        public Section Section { get; set; } = null!;

        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
