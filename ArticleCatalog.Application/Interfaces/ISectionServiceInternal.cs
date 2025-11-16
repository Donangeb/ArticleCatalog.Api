using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleCatalog.Application.Interfaces
{
    public interface ISectionServiceInternal
    {
        Task AssignArticleToSectionAsync(Guid articleId);
        Task CleanupSectionsAsync();
    }
}
