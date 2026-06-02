using Contracting.Shared.Dtos.MasterDtos.EngineerDto;

namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class GetTransferRequestActivityDto
    {
        public Guid Id { get; set; }
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
        public string? ActionType { get; set; }
        public string? Comments { get; set; }
        public GetEngineerDto? Engineer { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
