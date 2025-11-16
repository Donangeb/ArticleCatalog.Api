using ArticleCatalog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArticleCatalog.Api.Controllers;

[ApiController]
[Route("api/sections")]
public class SectionsController : ControllerBase
{
    private readonly ISectionService _service;

    public SectionsController(ISectionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetSectionsAsync();
        return Ok(result);
    }

    [HttpGet("{sectionId:guid}/articles")]
    public async Task<IActionResult> GetArticles(Guid sectionId)
    {
        var result = await _service.GetSectionArticlesAsync(sectionId);
        return Ok(result);
    }
}
