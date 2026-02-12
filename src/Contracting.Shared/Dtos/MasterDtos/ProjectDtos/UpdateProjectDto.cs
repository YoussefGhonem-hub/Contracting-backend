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
        public Guid? BranchId { get; set; }
        public bool hasSpecialFields { get; set; }
        public List<CreateProjectSpecialFieldDto> SpecialFields { get; set; } = new();
    }
}
