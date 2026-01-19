using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAssigneePerformanceByEngineerId
{
    public record GetAssigneePerformanceByEngineerIdQuery(Guid EngineerId) : IRequest<ErrorOr<AssigneePerformanceReportDto>>;
}
