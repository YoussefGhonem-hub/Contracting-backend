using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetWeeklyCompletion
{
    public record GetWeeklyCompletionQuery(int? Month = null, int? Year = null) : IRequest<ErrorOr<WeeklyCompletionReportDto>>;
}
