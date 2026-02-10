using Contracting.Shared.Dtos.MasterDtos.BranchDto;

namespace Contracting.Shared.Dtos.MasterDtos.ProjectDtos
{
    public class GetProjectDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? location { get; set; }
        public string? Code { get; set; }
        public string? imageUrl { get; set; }
        public Guid? BranchId { get; set; }
        public GetBranchDto? Branch { get; set; }
        public bool hasSpecialFields { get; set; }
        public List<ProjectSpecialFieldDto> SpecialFields { get; set; } = new();
    }
}
