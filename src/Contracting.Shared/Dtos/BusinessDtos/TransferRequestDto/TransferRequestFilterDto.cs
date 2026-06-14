namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class TransferRequestFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? StatusId { get; set; }
        public Guid? SourceProjectId { get; set; }
        public Guid? DestinationProjectId { get; set; }
        public Guid? RequestedById { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
    }
}
