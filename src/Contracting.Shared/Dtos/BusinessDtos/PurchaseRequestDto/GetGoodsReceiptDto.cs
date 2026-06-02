using Contracting.Shared.Dtos.MasterDtos.EngineerDto;

namespace Contracting.Shared.BusinessDtos.PurchaseRequestDto
{
    public class GetGoodsReceiptDto
    {
        public Guid Id { get; set; }
        public DateTime ReceiptDate { get; set; }
        public bool IsPartialReceipt { get; set; }
        public bool IsConfirmed { get; set; }
        public string? Notes { get; set; }
        public Guid? ReceivedById { get; set; }
        public GetEngineerDto? ReceivedBy { get; set; }
    }
}
