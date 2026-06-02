using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetDailyReportCompletion
{
    public record GetDailyReportCompletionQuery(int? Month, int? Year) : IRequest<ErrorOr<DailyReportCompletionDto>>;
}
