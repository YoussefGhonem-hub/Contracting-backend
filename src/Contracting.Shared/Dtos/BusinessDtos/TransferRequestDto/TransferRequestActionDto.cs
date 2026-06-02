namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class TransferRequestActionDto
    {
        /// <summary>ConfirmReceipt, ConfirmPartialReceipt, Cancel</summary>
        public string ActionType { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
}
