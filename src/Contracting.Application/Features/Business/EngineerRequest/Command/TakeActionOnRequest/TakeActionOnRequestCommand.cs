using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest
{
    public record TakeActionOnRequestCommand(
        Guid RequestId, 
        Guid EngineerId, 
        bool IsApproved, 
        string? ActionNote) : IRequest<ErrorOr<bool>>;
}