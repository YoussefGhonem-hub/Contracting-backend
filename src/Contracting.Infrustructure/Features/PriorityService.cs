using Contracting.Shared.Resources;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Infrustructure.Extensions;

namespace Contracting.Infrustructure.Features
{
    public class PriorityService : IPriorityService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public PriorityService(ApplicationDbContext db, IMapper mapper, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _mapper = mapper;
            _localizer = localizer;
        }

        // ---------------- CREATE ----------------
        public async Task<GetDropDownPriorityDto> CreatePriorityAsync(CreatePriorityDto dto)
        {
            var priority = _mapper.Map<Priority>(dto);

            await _db.Priorities.AddAsync(priority);
            await _db.SaveChangesAsync();

            return _mapper.Map<GetDropDownPriorityDto>(priority);
        }

        // ---------------- UPDATE ----------------
        public async Task<GetDropDownPriorityDto> UpdatePriorityAsync(UpdatePriorityDto dto)
        {
            var priority = await _db.Priorities
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (priority is null)
                return null!;

            // Update fields
            priority.nameEn = dto.nameEn;
            priority.nameAr = dto.nameAr;
            priority.code = dto.code;
            priority.iconName = dto.iconName;

            await _db.SaveChangesAsync();

            return _mapper.Map<GetDropDownPriorityDto>(priority);
        }

        // ---------------- DELETE ----------------
        public async Task<GenericResponse> DeletePriorityAsync(Guid priorityId)
        {
            var priority = await _db.Priorities.FindAsync(priorityId);
            if (priority is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.PriorityNotFound]);

            _db.Priorities.Remove(priority);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.PriorityDeleteSuccess]);
        }

        // ---------------- GET ALL WITH PAGINATION ----------------
        public async Task<PaginatedList<GetDropDownPriorityDto>> GetAllPrioritiesAsync(BaseFilterDto filter, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _db.Priorities.AsNoTracking();

                if (string.IsNullOrWhiteSpace(filter.Sort))
                {
                    query = query.OrderBy(p => p.CreatedDate);
                }
                else
                {
                    query = query.OrderByDynamic(filter.Sort, filter.Descending);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                if (totalCount == 0)
                {
                    return new PaginatedList<GetDropDownPriorityDto>(
                        new List<GetDropDownPriorityDto>(),
                        0,
                        filter.PageIndex,
                        filter.PageSize);
                }

                var priorities = await query
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync(cancellationToken);

                var priorityDtos = _mapper.Map<List<GetDropDownPriorityDto>>(priorities);

                return new PaginatedList<GetDropDownPriorityDto>(
                    priorityDtos,
                    totalCount,
                    filter.PageIndex,
                    filter.PageSize);
            }
            catch (Exception)
            {
                return new PaginatedList<GetDropDownPriorityDto>(
                    new List<GetDropDownPriorityDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }
        }

        // ---------------- DROPDOWN ----------------
        public async Task<List<GetDropDownPriorityDto>> GetPriorityDropdownAsync()
        {
            var priorities = await _db.Priorities
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<GetDropDownPriorityDto>>(priorities);
        }
    }
}