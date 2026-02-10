namespace Contracting.Shared.Dtos.MasterDtos.ProjectDtos
{
    public class ProjectSpecialFieldsCheckDto
    {
        public Guid ProjectId { get; set; }
        public bool hasSpecialFields { get; set; }
        public List<ProjectSpecialFieldDto> SpecialFields { get; set; } = new();
    }
}
