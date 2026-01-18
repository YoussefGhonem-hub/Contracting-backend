using Contracting.Shared.Dtos.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetOfficeEngineerAnalysis
{
    public record GetOfficeEngineerAnalysisQuery() : IRequest<ErrorOr<OfficeEngineerAnalysisDto>>;
}
