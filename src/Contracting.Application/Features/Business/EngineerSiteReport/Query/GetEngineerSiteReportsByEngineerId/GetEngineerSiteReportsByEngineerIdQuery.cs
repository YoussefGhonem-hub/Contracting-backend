using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Dtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportsByEngineerId
{
    public record GetEngineerSiteReportsByEngineerIdQuery(Guid EngineerId, BaseFilterDto Filter)
        : IRequest<ErrorOr<PaginatedList<GetEngineerSiteReportDto>>>;
}
