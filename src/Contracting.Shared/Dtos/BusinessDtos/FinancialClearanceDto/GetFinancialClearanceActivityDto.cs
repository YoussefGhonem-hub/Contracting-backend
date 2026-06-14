using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;

namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class GetFinancialClearanceActivityDto
    {
        public Guid Id { get; set; }
        public Guid? FromStatusId { get; set; }
        public GetDropDownStatusDto? FromStatus { get; set; }
        public Guid ToStatusId { get; set; }
        public GetDropDownStatusDto? ToStatus { get; set; }
        public string? ActionType { get; set; }
        public string? Comments { get; set; }
        public GetEngineerDto? Engineer { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
