using Contracting.Shared.Dtos.ClientDtos.ThreeDDtos;
using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Contracting.Infrustructure.Inteface.client;

public interface IThreeDFolderService
{
    Task<ErrorOr<GetThreeDFolderDto>>        CreateFolderAsync(Guid projectId, CreateThreeDFolderDto dto, CancellationToken ct = default);
    Task<ErrorOr<GetThreeDFolderDto>>        UpdateFolderAsync(Guid projectId, Guid folderId, UpdateThreeDFolderDto dto, CancellationToken ct = default);
    Task<ErrorOr<bool>>                      DeleteFolderAsync(Guid projectId, Guid folderId, CancellationToken ct = default);
    Task<ErrorOr<List<GetThreeDImageDto>>>   AddImagesAsync(Guid projectId, Guid folderId, List<IFormFile> files, CancellationToken ct = default);
    Task<ErrorOr<bool>>                      DeleteImageAsync(Guid projectId, Guid folderId, Guid imageId, CancellationToken ct = default);
    Task<ErrorOr<List<GetThreeDFolderDto>>>  GetFoldersAsync(Guid projectId, CancellationToken ct = default);
    Task<ErrorOr<GetThreeDFolderDetailDto>>  GetFolderDetailAsync(Guid projectId, Guid folderId, CancellationToken ct = default);
}
