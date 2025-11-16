using ArticleCatalog.Domain.Common;

namespace ArticleCatalog.Domain.ValueObjects;

/// <summary>
/// Value Object для ключа набора тегов (используется для группировки статей в разделы)
/// </summary>
public class TagSetKey : ValueObject
{
    public string Value { get; }

    private TagSetKey(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Создает ключ набора тегов из коллекции названий тегов
    /// </summary>
    public static TagSetKey Create(IEnumerable<string> tags)
    {
        var normalizedTags = tags
            .Select(t => t.Trim().ToLowerInvariant())
            .OrderBy(t => t)
            .ToList();

        var key = string.Join("|", normalizedTags);
        return new TagSetKey(key);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
