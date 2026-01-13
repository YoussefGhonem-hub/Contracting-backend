using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestCountByStatus
{
    public record GetEngineerRequestCountByStatusQuery(Guid EngineerId) : IRequest<ErrorOr<List<GetEngineerRequestCountByStatusDto>>>;
}
