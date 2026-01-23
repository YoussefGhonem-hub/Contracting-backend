using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteSurveyQuestion
{
    public class CreateEngineerSiteSurveyQuestionCommandHandler : IRequestHandler<CreateEngineerSiteSurveyQuestionCommand, ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>>
    {
        private readonly IEngineerSiteSurveyQuestionService _service;

        public CreateEngineerSiteSurveyQuestionCommandHandler(IEngineerSiteSurveyQuestionService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>> Handle(CreateEngineerSiteSurveyQuestionCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateQuestionAsync(request.Question, cancellationToken);

            return result is null
                ? Error.Failure("Could not create question.")
                : result;
        }
    }
}
