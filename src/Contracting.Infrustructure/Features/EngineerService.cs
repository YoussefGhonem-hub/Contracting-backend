using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.EngineerDto;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features
{
    public class EngineerService : IEngineerService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public EngineerService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<GetEngineerDto> CreateEngineerAsync(CreateEngineerDto dto)
        {
            var engineer = _mapper.Map<Engineer>(dto);
            await _db.Engineers.AddAsync(engineer);
            await _db.SaveChangesAsync();
            return _mapper.Map<GetEngineerDto>(engineer);
        }

        public async Task<GetEngineerDto> UpdateEngineerAsync(UpdateEngineerDto dto)
        {
            var engineer = await _db.Engineers.FindAsync(dto.Id);
            if (engineer is null)
                return null!;

            // Map updated fields
            engineer.nameEn = dto.nameEn;
            engineer.nameAr = dto.nameAr;
            engineer.address = dto.address;
            engineer.passportNumber = dto.passportNumber;
            engineer.nationalId = dto.nationalId;
            engineer.position = dto.position;

            // Change department if provided
            if (dto.ChangeDepartmentId.HasValue)
            {
                engineer.DepartmentId = dto.ChangeDepartmentId.Value;
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<GetEngineerDto>(engineer);
        }



        public async Task<bool> DeleteEngineerAsync(Guid engineerId)
        {
            var engineer = await _db.Engineers.FindAsync(engineerId);
            if (engineer is null)
                return false;

            _db.Engineers.Remove(engineer);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedList<GetEngineerDto>> GetEngineerListAsync(string departmentId, BaseFilterDto filter)
        {
            var query = _db.Engineers.Include(x=>x.Department).AsNoTracking();

            if (Guid.TryParse(departmentId, out var deptGuid))
            {
                query = query.Where(e => e.DepartmentId == deptGuid);
            }

            return await query.PaginateAsync<Engineer, GetEngineerDto>(filter.PageIndex, filter.PageSize);
        }

        public async Task<GetEngineerDto> GetEngineerByIdAsync(Guid engineerId)
        {
            var engineer = await _db.Engineers
                .Include(e => e.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == engineerId);

            return engineer is null ? null! : _mapper.Map<GetEngineerDto>(engineer);
        }

        public async Task<List<GetEngineerDropDownDto>> GetEngineerDropdownAsync()
        {
            var engineers = await _db.Engineers
               .Include(e => e.Department) // Ensure Department is eagerly loaded
               .AsNoTracking()
               .ToListAsync();

            return _mapper.Map<List<GetEngineerDropDownDto>>(engineers);
        }
    }
}
