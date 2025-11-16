using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleCatalog.Application.DTOs
{
    public record ArticleDto (
        Guid Id,
        string Title,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt,
        IReadOnlyList<string> Tags
    );
}
