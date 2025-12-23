namespace Contracting.Shared.MasterDtos.ProjectDtos
{
    public class UpdateProjectDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? location { get; set; }
        public Guid? BranchId { get; set; }
    }
}
