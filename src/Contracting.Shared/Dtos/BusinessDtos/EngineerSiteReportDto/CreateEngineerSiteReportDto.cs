using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class CreateEngineerSiteReportDto
    {
        public Guid? ProjectId { get; set; }
        public DateTimeOffset? ReportDate { get; set; }
        public string? GeneralNotes { get; set; }
        public string? SiteSafetyObservations { get; set; }
        public string? QualityControlObservations { get; set; }
        public ICollection<CreateEngineerSiteWorkLogDto>? WorkLogs { get; set; }
        public ICollection<CreateEngineerSiteMaterialDto>? Materials { get; set; }
        public ICollection<CreateEngineerSiteEquipmentDto>? Equipments { get; set; }
        public ICollection<CreateEngineerSiteSurveyQuestionDto>? SurveyQuestions { get; set; }
    }

    public class CreateEngineerSiteWorkLogDto
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public decimal? quantity { get; set; }
        public decimal? totalHours { get; set; }
        public decimal? totalHoursToDate { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
    }

    public class CreateEngineerSiteMaterialDto
    {
        public string? name { get; set; }
        public decimal? quantity { get; set; }
        public string? usage { get; set; }
        public bool? needMore { get; set; }
        public string? unit { get; set; }
        public decimal? unitCost { get; set; }
        public decimal? totalCost { get; set; }
        public string? notes { get; set; }
    }

    public class CreateEngineerSiteEquipmentDto
    {
        public string? name { get; set; }
        public decimal? quantity { get; set; }
        public decimal? hoursUsed { get; set; }
        public string? condition { get; set; }
        public bool? isOperational { get; set; }
        public string? notes { get; set; }
    }

    public class CreateEngineerSiteSurveyQuestionDto
    {
        public Guid? TemplateId { get; set; }
        public string? question { get; set; }
        public string? answer { get; set; }
        public string? description { get; set; }
    }
}
