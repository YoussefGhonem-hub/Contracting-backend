namespace Contracting.Shared.Dtos.MasterDtos.ProjectDtos
{
    public class ProjectSpecialFieldDto
    {
        public Guid Id { get; set; }
        public Guid SpecialFieldId { get; set; }
        public string? name { get; set; }
        public string? fieldType { get; set; }
        public string? value { get; set; }
    }
}