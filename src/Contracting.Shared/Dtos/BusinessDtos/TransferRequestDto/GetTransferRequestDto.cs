using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;

namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class GetTransferRequestDto
    {
        public Guid Id { get; set; }
        public string? RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public Guid? SourceProjectId { get; set; }
        public GetProjectDto? SourceProject { get; set; }
        public string? SourceWarehouse { get; set; }
        public Guid? DestinationProjectId { get; set; }
        public GetProjectDto? DestinationProject { get; set; }
        public string? DestinationWarehouse { get; set; }
        public Guid? RequestedById { get; set; }
        public GetEngineerDto? RequestedBy { get; set; }
        public string? Notes { get; set; }
        public Guid? StatusId { get; set; }
        public GetDropDownStatusDto? Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public List<GetTransferRequestItemDto> Items { get; set; } = new();
        public List<GetAttachmentDto> Attachments { get; set; } = new();
        public List<GetTransferRequestActivityDto> Activities { get; set; } = new();
        /// <summary>Activity comments projected into the same Notes shape used by other request types.</summary>
        public List<GetEngineerRequestNotesDto> EngineerRequestNotes { get; set; } = new();
        public bool NeedsAcknowledgment { get; set; }
    }
}
