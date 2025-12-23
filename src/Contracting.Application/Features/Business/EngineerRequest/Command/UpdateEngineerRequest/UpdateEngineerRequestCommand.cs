using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.UpdateEngineerRequest
{
    public record UpdateEngineerRequestCommand(UpdateEngineerRequestDto Request) : IRequest<ErrorOr<GetAllEngineerRequestDto>>;
}