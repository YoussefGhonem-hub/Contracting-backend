using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.DepartmentDtos;
using MapsterMapper;
using System.Data.Entity;

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
            var department =  _db.Departmentes
                .FirstOrDefault(d => d.Id == departmentDto.Id);

            if (department is null)
                return null!;

            department.nameEn = departmentDto.nameEn;
            department.nameAr = departmentDto.nameAr;
            // Map other fields as needed

            await _db.SaveChangesAsync();

            return _mapper.Map<GetDepartmentDto>(department);
        }

        public async Task<PaginatedList<GetDepartmentDto>> GetDepartmentsByBranchIdAsync(Guid branchId, BaseFilterDto filter)
        {
            var query = _db.Departmentes
                .Where(d => d.BranchId == branchId)
                .AsNoTracking();

            // Example pagination: pageIndex = 1, pageSize = 20
            return await query.PaginateAsync<Department, GetDepartmentDto>(filter.PageIndex, filter.PageSize);
        }

        public async Task<bool> RemoveDepartmentAsync(Guid branchId, Guid departmentId)
        {
            var department =  _db.Departmentes
                .FirstOrDefault(d => d.Id == departmentId && d.BranchId == branchId);

            if (department is null)
                return false;

            _db.Departmentes.Remove(department);
            await _db.SaveChangesAsync();
            return true;
        }
    }

}
