using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportById
{
    public record GetEngineerSiteReportByIdQuery(Guid ReportId) : IRequest<ErrorOr<GetEngineerSiteReportDto>>;
}
