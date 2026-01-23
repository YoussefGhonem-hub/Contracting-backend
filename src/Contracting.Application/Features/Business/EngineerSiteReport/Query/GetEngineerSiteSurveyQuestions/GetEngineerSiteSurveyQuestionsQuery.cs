using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteSurveyQuestions
{
    public record GetEngineerSiteSurveyQuestionsQuery(BaseFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetEngineerSiteSurveyQuestionTemplateDto>>>;
}
