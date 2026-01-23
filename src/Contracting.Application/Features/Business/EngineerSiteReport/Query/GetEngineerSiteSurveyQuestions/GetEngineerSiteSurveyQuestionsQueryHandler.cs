using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Infrustructure.Extensions.Helpers;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteSurveyQuestions
{
    public class GetEngineerSiteSurveyQuestionsQueryHandler : IRequestHandler<GetEngineerSiteSurveyQuestionsQuery, ErrorOr<PaginatedList<GetEngineerSiteSurveyQuestionTemplateDto>>>
    {
        private readonly IEngineerSiteSurveyQuestionService _service;

        public GetEngineerSiteSurveyQuestionsQueryHandler(IEngineerSiteSurveyQuestionService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetEngineerSiteSurveyQuestionTemplateDto>>> Handle(GetEngineerSiteSurveyQuestionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetQuestionsAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
