using Contracting.API.Controllers.Shared;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.TwoDDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/projects/{projectId:guid}/2d-folders")]
[ApiController]
[Authorize]
public class TwoDFolderController : APIBaseController
{
    private readonly ITwoDFolderService _service;

    public TwoDFolderController(ITwoDFolderService service)
    {
        _service = service;
    }

    // ── Project Manager endpoints ──────────────────────────────────────────────

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateFolder(
        Guid projectId,
        [FromForm] CreateTwoDFolderDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateFolderAsync(projectId, dto, cancellationToken);
        return result.Match(f => Ok(f), errors => Problem(errors));
    }

    [HttpPut("{folderId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateFolder(
        Guid projectId,
        Guid folderId,
        [FromForm] UpdateTwoDFolderDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateFolderAsync(projectId, folderId, dto, cancellationToken);
        return result.Match(f => Ok(f), errors => Problem(errors));
    }

    [HttpDelete("{folderId:guid}")]
    public async Task<IActionResult> DeleteFolder(
        Guid projectId,
        Guid folderId,
        CancellationToken cancellationToken)
    {
        var result = await _service.DeleteFolderAsync(projectId, folderId, cancellationToken);
        return result.Match(_ => NoContent(), errors => Problem(errors));
    }

    [HttpPost("{folderId:guid}/images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddImages(
        Guid projectId,
        Guid folderId,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var result = await _service.AddImagesAsync(projectId, folderId, files, cancellationToken);
        return result.Match(imgs => Ok(imgs), errors => Problem(errors));
    }

    [HttpDelete("{folderId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(
        Guid projectId,
        Guid folderId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result = await _service.DeleteImageAsync(projectId, folderId, imageId, cancellationToken);
        return result.Match(_ => NoContent(), errors => Problem(errors));
    }

    // ── Read endpoints (all authenticated users) ───────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetFolders(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetFoldersAsync(projectId, cancellationToken);
        return result.Match(f => Ok(f), errors => Problem(errors));
    }

    [HttpGet("{folderId:guid}")]
    public async Task<IActionResult> GetFolderDetail(
        Guid projectId,
        Guid folderId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetFolderDetailAsync(projectId, folderId, cancellationToken);
        return result.Match(f => Ok(f), errors => Problem(errors));
    }
}
