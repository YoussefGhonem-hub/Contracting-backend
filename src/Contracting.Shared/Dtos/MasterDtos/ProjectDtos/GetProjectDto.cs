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
        public Guid? BranchId { get; set; }
        public GetBranchDto? Branch { get; set; }
    }
}
