using Contracting.Shared.Dtos.ClientDtos.TwoDDtos;
using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Contracting.Infrustructure.Inteface.client;

public interface ITwoDFolderService
{
    Task<ErrorOr<GetTwoDFolderDto>>        CreateFolderAsync(Guid projectId, CreateTwoDFolderDto dto, CancellationToken ct = default);
    Task<ErrorOr<GetTwoDFolderDto>>        UpdateFolderAsync(Guid projectId, Guid folderId, UpdateTwoDFolderDto dto, CancellationToken ct = default);
    Task<ErrorOr<bool>>                    DeleteFolderAsync(Guid projectId, Guid folderId, CancellationToken ct = default);
    Task<ErrorOr<List<GetTwoDImageDto>>>   AddImagesAsync(Guid projectId, Guid folderId, List<IFormFile> files, CancellationToken ct = default);
    Task<ErrorOr<bool>>                    DeleteImageAsync(Guid projectId, Guid folderId, Guid imageId, CancellationToken ct = default);
    Task<ErrorOr<List<GetTwoDFolderDto>>>  GetFoldersAsync(Guid projectId, CancellationToken ct = default);
    Task<ErrorOr<GetTwoDFolderDetailDto>>  GetFolderDetailAsync(Guid projectId, Guid folderId, CancellationToken ct = default);
}
