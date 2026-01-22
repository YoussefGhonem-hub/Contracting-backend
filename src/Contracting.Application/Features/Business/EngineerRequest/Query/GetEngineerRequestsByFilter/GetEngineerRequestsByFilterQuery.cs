using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestsByFilter
{
    public record GetEngineerRequestsByFilterQuery(EngineerRequestFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>;
}
