namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetEngineerRequestCountByStatusDto
    {
        public Guid StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;
        /// <summary>Engineer request count for this status.</summary>
        public int Count { get; set; }
        /// <summary>Transfer request count for this status.</summary>
        public int TransferCount { get; set; }
        /// <summary>Labor attendance request count for this status.</summary>
        public int LaborCount { get; set; }
        /// <summary>Financial clearance request count for this status.</summary>
        public int FinancialClearanceCount { get; set; }
        /// <summary>Sum of all request types for this status — use this for dashboard totals.</summary>
        public int TotalCount => Count + TransferCount + LaborCount + FinancialClearanceCount;
    }
}
