using ArticleCatalog.Domain.Entities;

namespace ArticleCatalog.Domain.Tests.Helpers;

/// <summary>
/// Helper для создания Tag в тестах
/// </summary>
public static class TagTestHelper
{
    /// <summary>
    /// Создает Tag с установленным именем и нормализованным именем
    /// </summary>
    public static Tag CreateTag(Guid id, string name)
    {
        var tag = new Tag { Id = id };
        tag.SetName(name);
        return tag;
    }

    /// <summary>
    /// Создает Tag с автоматически сгенерированным ID
    /// </summary>
    public static Tag CreateTag(string name)
    {
        return CreateTag(Guid.NewGuid(), name);
    }
}

