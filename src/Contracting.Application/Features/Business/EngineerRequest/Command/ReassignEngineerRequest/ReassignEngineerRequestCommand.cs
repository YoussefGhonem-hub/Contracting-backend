using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.ReassignEngineerRequest
{
    public record ReassignEngineerRequestCommand(Guid RequestId, ReassignEngineerRequestDto Dto) : IRequest<ErrorOr<GenericResponse>>;
}
