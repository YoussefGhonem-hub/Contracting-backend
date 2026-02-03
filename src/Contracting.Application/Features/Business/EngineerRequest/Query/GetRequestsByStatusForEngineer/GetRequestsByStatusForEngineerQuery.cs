using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestsByStatusForEngineer
{
    public record GetRequestsByStatusForEngineerQuery(GetRequestsByStatusFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>;
}
