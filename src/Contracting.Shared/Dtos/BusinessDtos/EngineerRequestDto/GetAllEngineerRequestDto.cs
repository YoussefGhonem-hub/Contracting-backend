using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetAllEngineerRequestDto
    {
        public string RequestType { get; set; } = "EngineerRequest";
        public Guid? Id { get; set; }
        public Guid? ProjectId { get; set; }
        public GetProjectDto? Project { get; set; }
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
        public Guid? PriorityId { get; set; }
        public GetDropDownPriorityDto? Priority { get; set; }
        public Guid? EngineerId { get; set; }
        public GetEngineerDto? Engineer { get; set; }
        public Guid? requestedById { get; set; }
        public GetEngineerDto? requestedBy { get; set; }
        public Guid StatusId { get; set; }
        public GetDropDownStatusDto? Status { get; set; }
        public Guid? assignToId { get; set; }
        public GetEngineerDto? assignTo { get; set; }
        public string? RequestTitle { get; set; }
        public string? Descreption { get; set; }
        public int? timeDuration { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public ICollection<GetEngineerRequestNotesDto> EngineerRequestNotes { get; set; } = new List<GetEngineerRequestNotesDto>();
        public ICollection<GetEngineerRequestActiviteDto> EngineerRequestActivites { get; set; } = new List<GetEngineerRequestActiviteDto>();
        public ICollection<Contracting.Shared.Dtos.GetAttachmentDto>? EngineerRequestAttachments { get; set; } = new List<GetAttachmentDto>();
        public List<EngineerRequestSpecialFieldValueDto> SpecialFieldValues { get; set; } = new();
        public List<GetEngineerRequestSpecialFieldItemDto> SpecialFieldItems { get; set; } = new();

        // Goods receipts (populated when request is a Purchase Request for Procurement dept)
        public List<GetGoodsReceiptDto> Receipts { get; set; } = new();
        // True = office engineer confirmed completion, but site engineer receipt confirmation is still pending
        public bool NeedsReceiptConfirmation { get; set; }
    }
}
