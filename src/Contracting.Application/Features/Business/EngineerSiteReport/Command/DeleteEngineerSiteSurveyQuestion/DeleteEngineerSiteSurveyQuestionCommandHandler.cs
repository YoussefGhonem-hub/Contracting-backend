using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.DeleteEngineerSiteSurveyQuestion
{
    public class DeleteEngineerSiteSurveyQuestionCommandHandler : IRequestHandler<DeleteEngineerSiteSurveyQuestionCommand, ErrorOr<GenericResponse>>
    {
        private readonly IEngineerSiteSurveyQuestionService _service;

        public DeleteEngineerSiteSurveyQuestionCommandHandler(IEngineerSiteSurveyQuestionService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteEngineerSiteSurveyQuestionCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteQuestionAsync(request.Id, cancellationToken);

            return result;
        }
    }
}
