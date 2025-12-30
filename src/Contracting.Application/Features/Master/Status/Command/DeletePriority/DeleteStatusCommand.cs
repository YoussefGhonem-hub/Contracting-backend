using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Command.DeleteStatus
{
    public record DeleteStatusCommand(Guid StatusId) : IRequest<ErrorOr<GenericResponse>>;
}