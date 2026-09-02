using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.ClientDtos.TwoDDtos;

public class CreateTwoDFolderDto
{
    public string Title { get; set; } = default!;
    public IFormFile? CoverImage { get; set; }
}

public class UpdateTwoDFolderDto
{
    public string? Title { get; set; }
    public IFormFile? CoverImage { get; set; }
}

public class GetTwoDFolderDto
{
    public Guid    Id         { get; set; }
    public string  Title      { get; set; } = default!;
    public string? CoverUrl   { get; set; }
    public int     ImageCount { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class GetTwoDFolderDetailDto : GetTwoDFolderDto
{
    public List<GetTwoDImageDto> Images { get; set; } = new();
}

public class GetTwoDImageDto
{
    public Guid    Id         { get; set; }
    public string? FileName   { get; set; }
    public string? Extension  { get; set; }
    public long?   FileSize   { get; set; }
    public string? Url        { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
