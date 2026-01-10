namespace Contracting.Shared.Dtos
{
    public class GetAttachmentDto
    {
        public Guid Id { get; set; }
        public string? Key { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public long? FileSize { get; set; }
        public string? Url { get; set; }
    }
}
