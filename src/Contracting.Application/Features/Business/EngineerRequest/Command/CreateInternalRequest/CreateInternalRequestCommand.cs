using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.CreateInternalRequest
{
    public record CreateInternalRequestCommand(CreateInternalRequestDto Request) : IRequest<ErrorOr<GetAllEngineerRequestDto>>;
}
