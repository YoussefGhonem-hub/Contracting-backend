using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.DeletePriority
{
    public record DeletePriorityCommand(Guid PriorityId) : IRequest<ErrorOr<GenericResponse>>;
}