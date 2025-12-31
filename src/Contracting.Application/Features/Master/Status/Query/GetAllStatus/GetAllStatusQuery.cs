using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Query.GetAllStatus
{
    public record GetAllStatusQuery(BaseFilterDto Filter)
        : IRequest<ErrorOr<PaginatedList<GetDropDownStatusDto>>>;
}
