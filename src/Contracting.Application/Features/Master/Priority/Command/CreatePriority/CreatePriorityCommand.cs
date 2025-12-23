using Contracting.Shared.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.CreatePriority
{
    public record CreatePriorityCommand(CreatePriorityDto Priority) : IRequest<ErrorOr<GetDropDownPriorityDto>>;
}