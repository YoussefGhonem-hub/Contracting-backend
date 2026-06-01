using Contracting.Shared.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.MasterDtos.ProjectDtos
{
    public class UpdateProjectDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? location { get; set; }
        public string? Code { get; set; }
        public IFormFile? Image { get; set; }
        public decimal? Area { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? ExpectedEndDate { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public Guid? BranchId { get; set; }
    }
}
