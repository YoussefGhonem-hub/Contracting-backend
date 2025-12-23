using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.DeleteEngineerRequest
{
    public record DeleteEngineerRequestCommand(Guid RequestId) : IRequest<ErrorOr<bool>>;
}