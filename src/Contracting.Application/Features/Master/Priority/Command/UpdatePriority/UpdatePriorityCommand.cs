using Contracting.Shared.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.UpdatePriority
{
    public record UpdatePriorityCommand(UpdatePriorityDto Priority) : IRequest<ErrorOr<GetDropDownPriorityDto>>;
}