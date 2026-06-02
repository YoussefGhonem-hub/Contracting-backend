using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class CreateTransferRequestDto
    {
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public Guid? SourceProjectId { get; set; }
        public string? SourceWarehouse { get; set; }
        public Guid? DestinationProjectId { get; set; }
        public string? DestinationWarehouse { get; set; }
        public string? Notes { get; set; }
        public List<CreateTransferRequestItemDto> Items { get; set; } = new();
        public ICollection<IFormFile>? Attachments { get; set; }
    }
}
