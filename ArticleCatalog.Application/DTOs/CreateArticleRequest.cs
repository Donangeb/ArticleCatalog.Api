using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleCatalog.Application.DTOs
{
    public record CreateArticleRequest(
        string Title,
        List<string> Tags
    );
}
