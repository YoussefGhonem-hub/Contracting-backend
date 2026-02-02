using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportsByEngineerId
{
    public record GetEngineerSiteReportsByEngineerIdQuery(Guid EngineerId, EngineerSiteReportFilterDto Filter)
        : IRequest<ErrorOr<PaginatedList<GetEngineerSiteReportDto>>>;
}
