using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.UpdateEngineerSiteSurveyQuestion
{
    public record UpdateEngineerSiteSurveyQuestionCommand(UpdateEngineerSiteSurveyQuestionTemplateDto Question) : IRequest<ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>>;
}
