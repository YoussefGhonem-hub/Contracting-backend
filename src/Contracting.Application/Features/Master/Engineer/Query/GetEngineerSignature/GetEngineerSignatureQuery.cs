using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerSignature
{
    public record GetEngineerSignatureQuery(Guid EngineerId) : IRequest<ErrorOr<UserSignatureDto>>;
}
