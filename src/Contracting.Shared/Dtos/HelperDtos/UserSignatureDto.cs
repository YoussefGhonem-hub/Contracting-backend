namespace Contracting.Shared.Dtos.HelperDtos
{
    public class UserSignatureDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string SignatureUrl { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public long FileSize { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public DateTimeOffset? ModifiedDate { get; init; }
    }
}
