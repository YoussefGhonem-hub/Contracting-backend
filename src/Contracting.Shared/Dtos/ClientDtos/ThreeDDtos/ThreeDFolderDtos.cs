using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.ClientDtos.ThreeDDtos;

public class CreateThreeDFolderDto
{
    public string Title { get; set; } = default!;
    public IFormFile? CoverImage { get; set; }
}

public class UpdateThreeDFolderDto
{
    public string? Title { get; set; }
    public IFormFile? CoverImage { get; set; }
}

public class GetThreeDFolderDto
{
    public Guid    Id         { get; set; }
    public string  Title      { get; set; } = default!;
    public string? CoverUrl   { get; set; }
    public int     ImageCount { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class GetThreeDFolderDetailDto : GetThreeDFolderDto
{
    public List<GetThreeDImageDto> Images { get; set; } = new();
}

public class GetThreeDImageDto
{
    public Guid    Id         { get; set; }
    public string? FileName   { get; set; }
    public string? Extension  { get; set; }
    public long?   FileSize   { get; set; }
    public string? Url        { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
