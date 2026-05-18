using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using Contracting.Shared.Resources;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Contracting.Infrustructure.Features
{
    public class ConstructionItemService : IConstructionItemService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public ConstructionItemService(ApplicationDbContext db, IMapper mapper, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<GetConstructionItemDto> CreateAsync(CreateConstructionItemDto dto)
        {
            var entity = _mapper.Map<ConstructionItem>(dto);
            await _db.ConstructionItems.AddAsync(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<GetConstructionItemDto>(entity);
        }

        public async Task<GetConstructionItemDto> UpdateAsync(UpdateConstructionItemDto dto)
        {
            var entity = await _db.ConstructionItems.FindAsync(dto.Id);
            if (entity is null)
                return null!;

            entity.nameEn = dto.nameEn;
            entity.nameAr = dto.nameAr;
            entity.Unit = dto.Unit;
            entity.ItemCode = dto.ItemCode;

            await _db.SaveChangesAsync();
            return _mapper.Map<GetConstructionItemDto>(entity);
        }

        public async Task<GenericResponse> DeleteAsync(Guid id)
        {
            var entity = await _db.ConstructionItems.FindAsync(id);
            if (entity is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.NotFound]);

            _db.ConstructionItems.Remove(entity);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.DeleteSuccess]);
        }

        public async Task<GetConstructionItemDto> GetByIdAsync(Guid id)
        {
            var entity = await _db.ConstructionItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity is null ? null! : _mapper.Map<GetConstructionItemDto>(entity);
        }

        public async Task<PaginatedList<GetConstructionItemDto>> GetAllAsync(BaseFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.ConstructionItems.AsNoTracking();

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderBy(x => x.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new PaginatedList<GetConstructionItemDto>(
                    new List<GetConstructionItemDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var items = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<GetConstructionItemDto>>(items);

            return new PaginatedList<GetConstructionItemDto>(
                dtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        public async Task<List<GetConstructionItemDropdownDto>> GetDropdownAsync()
        {
            var items = await _db.ConstructionItems
                .AsNoTracking()
                .OrderBy(x => x.nameEn)
                .ToListAsync();

            return _mapper.Map<List<GetConstructionItemDropdownDto>>(items);
        }
    }
}
