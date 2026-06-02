using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class UpdateTransferRequestDto
    {
        public Guid Id { get; set; }
        public DateTime? RequestDate { get; set; }
        public Guid? SourceProjectId { get; set; }
        public string? SourceWarehouse { get; set; }
        public Guid? DestinationProjectId { get; set; }
        public string? DestinationWarehouse { get; set; }
        public string? Notes { get; set; }
        public List<CreateTransferRequestItemDto>? Items { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
    }
}
