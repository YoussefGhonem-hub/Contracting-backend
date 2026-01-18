using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetOfficeEngineerAnalysis
{
    public record GetOfficeEngineerAnalysisQuery() : IRequest<ErrorOr<OfficeEngineerAnalysisDto>>;
}
