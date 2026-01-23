using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;

namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class GetEngineerSiteReportDto
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public GetProjectDto? Project { get; set; }
        public Guid EngineerId { get; set; }
        public GetEngineerDto? Engineer { get; set; }
        public DateTimeOffset ReportDate { get; set; }
        public string? GeneralNotes { get; set; }
        public string? SiteSafetyObservations { get; set; }
        public string? QualityControlObservations { get; set; }
        public ICollection<GetEngineerSiteWorkLogDto> WorkLogs { get; set; } = new List<GetEngineerSiteWorkLogDto>();
        public ICollection<GetEngineerSiteMaterialDto> Materials { get; set; } = new List<GetEngineerSiteMaterialDto>();
        public ICollection<GetEngineerSiteEquipmentDto> Equipments { get; set; } = new List<GetEngineerSiteEquipmentDto>();
        public ICollection<GetEngineerSiteSurveyQuestionDto> SurveyQuestions { get; set; } = new List<GetEngineerSiteSurveyQuestionDto>();
    }

    public class GetEngineerSiteWorkLogDto
    {
        public Guid Id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public decimal? quantity { get; set; }
        public decimal? totalHours { get; set; }
        public decimal? totalHoursToDate { get; set; }
        public ICollection<GetAttachmentDto>? Attachments { get; set; } = new List<GetAttachmentDto>();
    }

    public class GetEngineerSiteMaterialDto
    {
        public Guid Id { get; set; }
        public string? name { get; set; }
        public decimal? quantity { get; set; }
        public string? usage { get; set; }
        public bool? needMore { get; set; }
        public string? unit { get; set; }
        public decimal? unitCost { get; set; }
        public decimal? totalCost { get; set; }
        public string? notes { get; set; }
    }

    public class GetEngineerSiteEquipmentDto
    {
        public Guid Id { get; set; }
        public string? name { get; set; }
        public decimal? quantity { get; set; }
        public decimal? hoursUsed { get; set; }
        public string? condition { get; set; }
        public bool? isOperational { get; set; }
        public string? notes { get; set; }
    }

    public class GetEngineerSiteSurveyQuestionDto
    {
        public Guid Id { get; set; }
        public Guid? TemplateId { get; set; }
        public string? question { get; set; }
        public string? answer { get; set; }
        public string? description { get; set; }
    }
}
