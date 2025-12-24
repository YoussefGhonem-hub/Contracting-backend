using Contracting.Shared.MasterDtos.PriorityDto;
using Contracting.Shared.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.UpdatePriority
{
    public record UpdatePriorityCommand(UpdatePriorityDto Priority) : IRequest<ErrorOr<GetDropDownPriorityDto>>;
}