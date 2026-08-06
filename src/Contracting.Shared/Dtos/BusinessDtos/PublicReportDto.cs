using Contracting.Shared.BusinessDtos.EngineerRequestDto;

namespace Contracting.Shared.BusinessDtos
{
    /// <summary>
    /// Envelope returned by every anonymous printable-report endpoint
    /// (`GET /api/{Controller}/public/{id}`): the request payload plus the
    /// creator and deciding-engineer signatories with their stored signatures.
    /// </summary>
    public class PublicReportDto<T>
    {
        public T Request { get; set; } = default!;
        public DateTimeOffset CreatedDate { get; set; }
        public RequestSignatoryDto? Creator { get; set; }
        public RequestSignatoryDto? Approver { get; set; }
    }
}
