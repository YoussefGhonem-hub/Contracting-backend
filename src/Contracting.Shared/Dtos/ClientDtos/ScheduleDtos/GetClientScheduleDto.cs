namespace Contracting.Shared.Dtos.ClientDtos.ScheduleDtos;

public class GetClientScheduleDto
{
    // Summary card
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? CompletionDate { get; set; }
    /// <summary>Duration in whole weeks between StartDate and CompletionDate. Null if either date is missing.</summary>
    public int? DurationWeeks { get; set; }
    public int? ProgressPercent { get; set; }

    // Tab 1 — Monthly Reports (flat list of downloadable report attachments)
    public List<ScheduleMonthlyReportFileDto> MonthlyReports { get; set; } = new();

    // Tab 2 — Project Timeline (uploaded schedule/Gantt files)
    public List<ScheduleTimelineDocumentDto> TimelineDocuments { get; set; } = new();
}

public class ScheduleMonthlyReportFileDto
{
    public Guid Id { get; set; }
    /// <summary>Month number (1–12)</summary>
    public int Month { get; set; }
    public int Year { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}

public class ScheduleTimelineDocumentDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
