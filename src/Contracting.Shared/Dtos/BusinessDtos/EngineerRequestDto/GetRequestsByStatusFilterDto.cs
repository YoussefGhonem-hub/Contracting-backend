using Contracting.Shared.Dtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetRequestsByStatusFilterDto : BaseFilterDto
    {
        public Guid StatusId { get; set; }
    }
}
