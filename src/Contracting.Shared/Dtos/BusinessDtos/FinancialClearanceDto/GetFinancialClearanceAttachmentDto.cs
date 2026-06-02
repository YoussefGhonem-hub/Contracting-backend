using Contracting.Shared.Dtos;

namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class GetFinancialClearanceAttachmentDto : GetAttachmentDto
    {
        public string? AttachmentType { get; set; }
    }
}
