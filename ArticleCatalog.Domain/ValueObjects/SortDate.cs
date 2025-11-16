using ArticleCatalog.Domain.Common;

namespace ArticleCatalog.Domain.ValueObjects;

/// <summary>
/// Value Object для даты сортировки статей (используется для определения приоритета при сортировке)
/// </summary>
public class SortDate : ValueObject
{
    public DateTimeOffset Value { get; }

    private SortDate(DateTimeOffset value)
    {
        Value = value;
    }

    /// <summary>
    /// Создает дату сортировки из даты создания и даты обновления
    /// Если есть дата обновления, используется она, иначе дата создания
    /// </summary>
    public static SortDate Resolve(DateTimeOffset created, DateTimeOffset? updated)
    {
        return new SortDate(updated ?? created);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
