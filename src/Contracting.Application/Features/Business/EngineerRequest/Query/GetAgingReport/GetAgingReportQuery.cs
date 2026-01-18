using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAgingReport
{
    public record GetAgingReportQuery() : IRequest<ErrorOr<AgingReportDto>>;
}
