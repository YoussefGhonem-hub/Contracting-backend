using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Command.DeleteClient
{
    public record DeleteClientCommand(Guid ClientId) : IRequest<ErrorOr<GenericResponse>>;
}
