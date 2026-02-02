using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetMyEngineerSiteReports
{
    public record GetMyEngineerSiteReportsQuery(EngineerSiteReportFilterDto Filter) : IRequest<ErrorOr<List<GetEngineerSiteReportDto>>>;
}
