using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.UpdateEngineerSiteSurveyQuestion
{
    public class UpdateEngineerSiteSurveyQuestionCommandHandler : IRequestHandler<UpdateEngineerSiteSurveyQuestionCommand, ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>>
    {
        private readonly IEngineerSiteSurveyQuestionService _service;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public UpdateEngineerSiteSurveyQuestionCommandHandler(IEngineerSiteSurveyQuestionService service, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>> Handle(UpdateEngineerSiteSurveyQuestionCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateQuestionAsync(request.Question, cancellationToken);

            return result is null
                ? Error.NotFound(_localizer[SharedResourcesKeys.SurveyQuestionNotFound])
                : result;
        }
    }
}
