using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.DeleteEngineerSiteSurveyQuestion
{
    public record DeleteEngineerSiteSurveyQuestionCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
}
