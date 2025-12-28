using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.BranchDto;
using Contracting.Shared.MasterDtos.DepartmentDtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public DepartmentService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<GetDepartmentDto> CreateDepartmentAsync(Guid branchId, CreateDepartmentDto departmentDto)
        {
            var branch = await _db.Branches.FindAsync(branchId);
            if (branch is null)
                return null!;

            var department = _mapper.Map<Department>(departmentDto);
            department.BranchId = branchId;
            

            await _db.Departmentes.AddAsync(department);
            await _db.SaveChangesAsync();

            return _mapper.Map<GetDepartmentDto>(department);
        }

        public async Task<GetDepartmentDto> UpdateDepartmentAsync(UpdateDepartmentDto departmentDto)
        {
            var department = await _db.Departmentes  // ✅ FIXED: Make async
                .FirstOrDefaultAsync(d => d.Id == departmentDto.Id);

            if (department is null)
                return null!;

            department.nameEn = departmentDto.nameEn;
            department.nameAr = departmentDto.nameAr;           

            await _db.SaveChangesAsync();

            return _mapper.Map<GetDepartmentDto>(department);
        }

        public async Task<PaginatedList<GetDepartmentDto>> GetDepartmentsByBranchIdAsync(
            Guid branchId,
            BaseFilterDto filter,
            CancellationToken cancellationToken)
        {
            var query = _db.Departmentes
                .Where(d => d.BranchId == branchId)
                .AsNoTracking();

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderBy(d => d.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and materialize the data
            var departments = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs after materialization (not in LINQ projection)
            var departmentDtos = _mapper.Map<List<GetDepartmentDto>>(departments);

            // Return paginated result
            return new PaginatedList<GetDepartmentDto>(
                departmentDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        public async Task<bool> RemoveDepartmentAsync(Guid branchId, Guid departmentId)
        {
            var department = await _db.Departmentes  // ✅ FIXED: Make async
                .FirstOrDefaultAsync(d => d.Id == departmentId && d.BranchId == branchId);

            if (department is null)
                return false;

            _db.Departmentes.Remove(department);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<GetDepartmentDto>> DropDownMethodAsync(Guid branchId)
        {
            return await _db.Departmentes
                   .Where(x => x.BranchId == branchId)
                   .Select(x => new GetDepartmentDto
                   {
                       Id = x.Id,
                       nameAr = x.nameAr,
                       nameEn = x.nameEn
                   })
                   .ToListAsync();
        }
    }

}
