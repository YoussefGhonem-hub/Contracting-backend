using Contracting.Shared.Resources;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;

namespace Contracting.Infrustructure.Features
{
    public class StatueService : IStatueService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public StatueService(ApplicationDbContext db, IMapper mapper, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _mapper = mapper;
            _localizer = localizer;
        }

        // ---------------- CREATE ----------------
        public async Task<GetDropDownStatusDto> CreateStatusAsync(CreateStatusDto dto)
        {
            var Status = _mapper.Map<Status>(dto);

            await _db.Statuses.AddAsync(Status);
            await _db.SaveChangesAsync();

            return _mapper.Map<GetDropDownStatusDto>(Status);
        }

        // ---------------- UPDATE ----------------
        public async Task<GetDropDownStatusDto> UpdateStatusAsync(UpdateStatusDto dto)
        {
            var Status = await _db.Statuses
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (Status is null)
                return null!;

            // Update fields
            Status.nameEn = dto.nameEn;
            Status.nameAr = dto.nameAr;
            Status.Code = dto.Code;

            await _db.SaveChangesAsync();

            return _mapper.Map<GetDropDownStatusDto>(Status);
        }

        // ---------------- DELETE ----------------
        public async Task<GenericResponse> DeleteStatusAsync(Guid StatusId)
        {
            var Status = await _db.Statuses.FindAsync(StatusId);
            if (Status is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.StatusNotFound]);

            _db.Statuses.Remove(Status);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.StatusDeleteSuccess]);
        }

        // ---------------- DROPDOWN ----------------
        public async Task<List<GetDropDownStatusDto>> GetStatusDropdownAsync()
        {
            var statues = await _db.Statuses
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<GetDropDownStatusDto>>(statues);
        }
    }
}