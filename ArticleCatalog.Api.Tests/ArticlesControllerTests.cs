using ArticleCatalog.Application.DTOs;
using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Api.Tests;

/// <summary>
/// Тесты для ArticlesController (API слой)
/// </summary>
public class ArticlesControllerTests
{
    private readonly Mock<IArticleService> _serviceMock;
    private readonly Mock<ILogger<Controllers.ArticlesController>> _loggerMock;
    private readonly Controllers.ArticlesController _controller;

    public ArticlesControllerTests()
    {
        _serviceMock = new Mock<IArticleService>();
        _loggerMock = new Mock<ILogger<Controllers.ArticlesController>>();
        _controller = new Controllers.ArticlesController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Get_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var articleDto = new ArticleDto(
            articleId,
            "Test Article",
            DateTimeOffset.UtcNow,
            null,
            new[] { "tag1", "tag2" }
        );

        _serviceMock.Setup(x => x.GetAsync(articleId))
            .ReturnsAsync(articleDto);

        // Act
        var result = await _controller.Get(articleId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(articleDto);
        _serviceMock.Verify(x => x.GetAsync(articleId), Times.Once);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _serviceMock.Setup(x => x.GetAsync(articleId))
            .ThrowsAsync(new NotFoundException($"Article {articleId} not found"));

        // Act
        var result = await _controller.Get(articleId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().NotBeNull();
        notFoundResult.Value.Should().BeAssignableTo<object>();
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateArticleRequest("Test Article", new List<string> { "tag1", "tag2" });
        var articleDto = new ArticleDto(
            Guid.NewGuid(),
            "Test Article",
            DateTimeOffset.UtcNow,
            null,
            new[] { "tag1", "tag2" }
        );

        _serviceMock.Setup(x => x.CreateAsync(request))
            .ReturnsAsync(articleDto);

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(articleDto);
        createdResult.ActionName.Should().Be(nameof(_controller.Get));
        _serviceMock.Verify(x => x.CreateAsync(request), Times.Once);
    }

    [Fact]
    public async Task Create_WithNullRequest_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _serviceMock.Verify(x => x.CreateAsync(It.IsAny<CreateArticleRequest>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateArticleRequest("", new List<string> { "tag1" });

        _serviceMock.Setup(x => x.CreateAsync(request))
            .ThrowsAsync(new ValidationException("Title is required"));

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
        badRequestResult.Value.Should().BeAssignableTo<object>();
    }

    [Fact]
    public async Task Update_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Updated Title", new List<string> { "tag1", "tag3" });
        var articleDto = new ArticleDto(
            articleId,
            "Updated Title",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            new[] { "tag1", "tag3" }
        );

        _serviceMock.Setup(x => x.UpdateAsync(articleId, request))
            .ReturnsAsync(articleDto);

        // Act
        var result = await _controller.Update(articleId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(articleDto);
        _serviceMock.Verify(x => x.UpdateAsync(articleId, request), Times.Once);
    }

    [Fact]
    public async Task Update_WithNullRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        // Act
        var result = await _controller.Update(articleId, null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _serviceMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateArticleRequest>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Updated Title", new List<string> { "tag1" });

        _serviceMock.Setup(x => x.UpdateAsync(articleId, request))
            .ThrowsAsync(new NotFoundException($"Article {articleId} not found"));

        // Act
        var result = await _controller.Update(articleId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _serviceMock.Setup(x => x.DeleteAsync(articleId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(articleId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(x => x.DeleteAsync(articleId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _serviceMock.Setup(x => x.DeleteAsync(articleId))
            .ThrowsAsync(new NotFoundException($"Article {articleId} not found"));

        // Act
        var result = await _controller.Delete(articleId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Get_WithException_ShouldReturnInternalServerError()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _serviceMock.Setup(x => x.GetAsync(articleId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Get(articleId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}

