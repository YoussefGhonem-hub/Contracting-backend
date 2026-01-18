using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetSlaBucketsReport
{
    public record GetSlaBucketsReportQuery() : IRequest<ErrorOr<SlaBucketsReportDto>>;
}
