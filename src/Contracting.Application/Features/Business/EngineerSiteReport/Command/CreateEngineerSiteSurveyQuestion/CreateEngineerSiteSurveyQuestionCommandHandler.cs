using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteSurveyQuestion
{
    public class CreateEngineerSiteSurveyQuestionCommandHandler : IRequestHandler<CreateEngineerSiteSurveyQuestionCommand, ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>>
    {
        private readonly IEngineerSiteSurveyQuestionService _service;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public CreateEngineerSiteSurveyQuestionCommandHandler(IEngineerSiteSurveyQuestionService service, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>> Handle(CreateEngineerSiteSurveyQuestionCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateQuestionAsync(request.Question, cancellationToken);

            return result is null
                ? Error.Failure(_localizer[SharedResourcesKeys.SurveyQuestionCreateFailed])
                : result;
        }
    }
}
