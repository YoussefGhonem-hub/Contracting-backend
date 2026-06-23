using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAllInternalRequests
{
    public record GetAllInternalRequestsQuery(InternalRequestFilterDto Filter)
        : IRequest<PaginatedList<GetAllEngineerRequestDto>>;
}
