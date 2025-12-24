using Contracting.Shared.MasterDtos.PriorityDto;
using Contracting.Shared.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Query.GetPriorityDropdown
{
    public record GetPriorityDropdownQuery() : IRequest<ErrorOr<List<GetDropDownPriorityDto>>>;
}