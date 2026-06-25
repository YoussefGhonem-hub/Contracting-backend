using Contracting.Shared.BusinessDtos.UnifiedRequestDto;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetRequestsByStatusResponseDto
    {
        // ── Paginated items (all request types for the selected status) ──
        public List<GetUnifiedRequestDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int FirstItemOnPage { get; set; }
        public int LastItemOnPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }

        // ── Count summary for the requested status (mirrors request-count-by-status API) ──
        public int EngineerRequestCount { get; set; }
        public int TransferCount { get; set; }
        public int LaborCount { get; set; }
        public int FinancialClearanceCount { get; set; }
        public int StatusTotalCount => EngineerRequestCount + TransferCount + LaborCount + FinancialClearanceCount;
    }
}
