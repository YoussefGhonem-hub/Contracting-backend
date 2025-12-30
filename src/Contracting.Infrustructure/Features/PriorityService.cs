using Contracting.Shared.Resources;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.MasterDtos.PriorityDto;
using Contracting.Shared.MasterDtos.StatusDtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

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