using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestStatusPercentage
{
    public record GetRequestStatusPercentageQuery(DateTime? StartDate = null, DateTime? EndDate = null) : IRequest<ErrorOr<EngineerStatusPercentageReportDto>>;
}
