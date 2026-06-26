using Contracting.Shared.Common.Enums;

namespace Contracting.Shared.Dtos.ClientDtos.ProjectDtos;

public class GetClientProjectListItemDto
{
    public Guid Id { get; set; }
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Area { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? ExpectedEndDate { get; set; }
    public ProjectStatus? ProjectStatus { get; set; }
    public string? Code { get; set; }
    public Guid? BranchId { get; set; }
    public string? Currency { get; set; }
}
