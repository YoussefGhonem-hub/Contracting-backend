using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Dtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetMyEngineerSiteReports
{
    public record GetMyEngineerSiteReportsQuery(BaseFilterDto Filter) : IRequest<ErrorOr<List<GetEngineerSiteReportDto>>>;
}
