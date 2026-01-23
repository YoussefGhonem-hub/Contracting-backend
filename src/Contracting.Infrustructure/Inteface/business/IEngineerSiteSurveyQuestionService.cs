using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerSiteSurveyQuestionService
    {
        Task<List<GetEngineerSiteSurveyQuestionTemplateDto>> GetActiveQuestionsAsync(CancellationToken cancellationToken = default);
        Task<PaginatedList<GetEngineerSiteSurveyQuestionTemplateDto>> GetQuestionsAsync(BaseFilterDto filter, CancellationToken cancellationToken = default);
        Task<GetEngineerSiteSurveyQuestionTemplateDto> CreateQuestionAsync(CreateEngineerSiteSurveyQuestionTemplateDto dto, CancellationToken cancellationToken = default);
        Task<GetEngineerSiteSurveyQuestionTemplateDto> UpdateQuestionAsync(UpdateEngineerSiteSurveyQuestionTemplateDto dto, CancellationToken cancellationToken = default);
        Task<GenericResponse> DeleteQuestionAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
