using Contracting.Shared.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Query.GetPriorityDropdown
{
    public record GetPriorityDropdownQuery() : IRequest<ErrorOr<List<GetDropDownPriorityDto>>>;
}