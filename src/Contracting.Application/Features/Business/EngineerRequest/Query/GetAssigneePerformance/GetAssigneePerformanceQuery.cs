using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAssigneePerformance
{
    public record GetAssigneePerformanceQuery() : IRequest<ErrorOr<AssigneePerformanceReportDto>>;
}
