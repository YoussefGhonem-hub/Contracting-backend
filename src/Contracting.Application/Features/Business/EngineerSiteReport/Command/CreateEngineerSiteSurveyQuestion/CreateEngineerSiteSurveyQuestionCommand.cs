using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteSurveyQuestion
{
    public record CreateEngineerSiteSurveyQuestionCommand(CreateEngineerSiteSurveyQuestionTemplateDto Question) : IRequest<ErrorOr<GetEngineerSiteSurveyQuestionTemplateDto>>;
}
