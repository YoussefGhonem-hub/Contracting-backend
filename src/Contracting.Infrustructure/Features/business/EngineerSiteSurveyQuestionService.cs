using Contracting.Domain.Entities.business;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.business
{
    public class EngineerSiteSurveyQuestionService : IEngineerSiteSurveyQuestionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public EngineerSiteSurveyQuestionService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<GetEngineerSiteSurveyQuestionTemplateDto>> GetActiveQuestionsAsync(CancellationToken cancellationToken = default)
        {
            var questions = await _db.EngineerSiteSurveyQuestionTemplates
                .Where(q => q.isActive)
                .OrderBy(q => q.order)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<GetEngineerSiteSurveyQuestionTemplateDto>>(questions);
        }

        public async Task<PaginatedList<GetEngineerSiteSurveyQuestionTemplateDto>> GetQuestionsAsync(BaseFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.EngineerSiteSurveyQuestionTemplates
                .AsNoTracking();

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderBy(q => q.order);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            if (totalCount == 0)
            {
                return new PaginatedList<GetEngineerSiteSurveyQuestionTemplateDto>(
                    new List<GetEngineerSiteSurveyQuestionTemplateDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var items = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<GetEngineerSiteSurveyQuestionTemplateDto>>(items);

            return new PaginatedList<GetEngineerSiteSurveyQuestionTemplateDto>(
                dtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        public async Task<GetEngineerSiteSurveyQuestionTemplateDto> CreateQuestionAsync(CreateEngineerSiteSurveyQuestionTemplateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new EngineerSiteSurveyQuestionTemplate
            {
                question = dto.question,
                order = dto.order,
                isActive = dto.isActive
            };

            await _db.EngineerSiteSurveyQuestionTemplates.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return _mapper.Map<GetEngineerSiteSurveyQuestionTemplateDto>(entity);
        }

        public async Task<GetEngineerSiteSurveyQuestionTemplateDto> UpdateQuestionAsync(UpdateEngineerSiteSurveyQuestionTemplateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _db.EngineerSiteSurveyQuestionTemplates
                .FirstOrDefaultAsync(q => q.Id == dto.Id, cancellationToken);

            if (entity is null)
                return null!;

            entity.question = dto.question;
            entity.order = dto.order;
            entity.isActive = dto.isActive;

            await _db.SaveChangesAsync(cancellationToken);

            return _mapper.Map<GetEngineerSiteSurveyQuestionTemplateDto>(entity);
        }

        public async Task<GenericResponse> DeleteQuestionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.EngineerSiteSurveyQuestionTemplates
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

            if (entity is null)
                return GenericResponse.FailureResult("Question not found");

            _db.EngineerSiteSurveyQuestionTemplates.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return GenericResponse.SuccessResult("Deleted successfully");
        }
    }
}
