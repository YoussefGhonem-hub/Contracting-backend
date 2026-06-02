namespace Contracting.Domain.Entities.business.enums
{
    public enum TransferRequestStatus
    {
        Draft = 0,
        PendingReceipt = 1,
        PartiallyReceived = 2,
        Closed = 3,
        Cancelled = 4
    }
}
