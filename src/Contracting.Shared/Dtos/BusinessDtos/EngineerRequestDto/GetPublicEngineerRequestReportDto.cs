namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    // Payload for the public (anonymous) printable request report.
    public class GetPublicEngineerRequestReportDto
    {
        public GetAllEngineerRequestDto Request { get; set; } = new();
        public DateTimeOffset CreatedDate { get; set; }
        public RequestSignatoryDto? Creator { get; set; }
        public RequestSignatoryDto? Approver { get; set; }
    }

    public class RequestSignatoryDto
    {
        public Guid? EngineerId { get; set; }
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }
        public string? Position { get; set; }
        // Pre-signed URL — re-issued on every call because S3 links expire
        public string? SignatureUrl { get; set; }
        public DateTimeOffset? SignedDate { get; set; }
        // "Created" for the creator; "Approved" / "Rejected" for the deciding engineer
        public string? Decision { get; set; }
    }
}
