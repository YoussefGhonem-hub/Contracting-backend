using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest
{
    public record TakeActionRequestCommand(
        Guid RequestId,
        TakeActionRequestDto ActionDto
    ) : IRequest<ErrorOr<GenericResponse>>;
}