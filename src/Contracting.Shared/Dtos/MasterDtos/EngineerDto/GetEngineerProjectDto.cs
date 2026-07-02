using Contracting.Shared.Common.Enums;
using Contracting.Shared.Dtos.MasterDtos.BranchDto;

namespace Contracting.Shared.Dtos.MasterDtos.EngineerDto
{
    public class GetEngineerProjectDto
    {
        public Guid ProjectId { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? location { get; set; }
        public string? Code { get; set; }
        public string? imageUrl { get; set; }
        public Guid? BranchId { get; set; }
        public GetBranchDto? Branch { get; set; }
        public bool IsProjectManager { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public List<string> Features { get; set; } = new();
    }
}
