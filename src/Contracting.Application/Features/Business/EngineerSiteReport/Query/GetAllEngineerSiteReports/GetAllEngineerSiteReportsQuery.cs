using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetAllEngineerSiteReports
{
    public record GetAllEngineerSiteReportsQuery(EngineerSiteReportFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetEngineerSiteReportDto>>>;
}
