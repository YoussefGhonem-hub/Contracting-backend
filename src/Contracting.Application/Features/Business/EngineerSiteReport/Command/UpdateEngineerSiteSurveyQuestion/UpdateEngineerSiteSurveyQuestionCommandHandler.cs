using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.UpdateEngineerSiteSurveyQuestion
{
    public class UpdateEngineerSiteSurveyQuestionCommandHandler : IRequestHandler<UpdateEngineerSiteSurveyQuestionCommand, ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>>
    {
        private readonly IEngineerSiteSurveyQuestionService _service;

        public UpdateEngineerSiteSurveyQuestionCommandHandler(IEngineerSiteSurveyQuestionService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>> Handle(UpdateEngineerSiteSurveyQuestionCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateQuestionAsync(request.Question, cancellationToken);

            return result is null
                ? Error.NotFound("Question not found.")
                : result;
        }
    }
}
