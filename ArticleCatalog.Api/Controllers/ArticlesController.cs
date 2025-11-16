using ArticleCatalog.Application.DTOs;
using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ArticleCatalog.Api.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _service;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(IArticleService service, ILogger<ArticlesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var result = await _service.GetAsync(id);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Article {ArticleId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting article {ArticleId}", id);
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest req)
    {
        try
        {
            if (req == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _service.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating article");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleRequest req)
    {
        try
        {
            if (req == null)
                return BadRequest(new { error = "Request body is required." });

            var result = await _service.UpdateAsync(id, req);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Article {ArticleId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating article {ArticleId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Article {ArticleId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
}
