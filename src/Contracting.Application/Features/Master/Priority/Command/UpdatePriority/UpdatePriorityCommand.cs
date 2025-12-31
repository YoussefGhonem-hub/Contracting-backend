using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.UpdatePriority
{
    public record UpdatePriorityCommand(UpdatePriorityDto Priority) : IRequest<ErrorOr<GetDropDownPriorityDto>>;
}