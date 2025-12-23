using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest
{
    public record CreateEngineerRequestCommand(CreateEngineerRequestDto Request) : IRequest<ErrorOr<GetAllEngineerRequestDto>>;
}