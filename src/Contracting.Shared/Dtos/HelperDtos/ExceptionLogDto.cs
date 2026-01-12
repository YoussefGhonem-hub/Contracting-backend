namespace Contracting.Shared.Dtos.HelperDtos
{
    public class ExceptionLogDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = default!;
        public string? StackTrace { get; set; }
        public string? InnerExceptionMessage { get; set; }
        public string? InnerExceptionStackTrace { get; set; }
        public string? ExceptionType { get; set; }
        public string? HttpMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? QueryString { get; set; }
        public string? UserId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public int? StatusCode { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
