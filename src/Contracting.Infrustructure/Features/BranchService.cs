using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.BranchDto;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public BranchService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ---------------- CREATE ----------------
        public async Task<GetBranchDto> AddAsync(CreateBranchDto dto)
        {
            var branch = _mapper.Map<Branch>(dto);
            // At this point, branch.Departments is already populated by the mapper

            // Set the BranchId for each department (if not set by the mapper)
            foreach (var department in branch.Departments)
            {
                department.BranchId = branch.Id;
            }

            await _db.Branches.AddAsync(branch);
            await _db.SaveChangesAsync();

            return _mapper.Map<GetBranchDto>(branch);
        }

        // ---------------- DELETE ----------------
        public async Task DeleteAsync(Guid id)
        {
            var branch = await _db.Branches.FindAsync(id);
            
            // Check for related departments
            bool hasDepartments = await _db.Departmentes.AnyAsync(d => d.Branch.Id == id);
            if (hasDepartments)
                throw new InvalidOperationException("Cannot delete branch with existing departments."); // Or return an error

            _db.Branches.Remove(branch);
            await _db.SaveChangesAsync();
        }

        // ---------------- GET ALL (PAGINATION) ----------------
        public async Task<PaginatedList<GetBranchDto>> GetAllAsync(
            BaseFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Branches.Include(b => b.Departments)
                .AsNoTracking()
                .AsQueryable();

            query = query.OrderByDynamic(filter.Sort, filter.Descending);

            return await query.PaginateAsync<Branch, GetBranchDto>(
                filter.PageIndex,
                filter.PageSize,
                cancellationToken);
        }

        // ---------------- GET BY ID ----------------
        public async Task<GetBranchDto> GetByIdAsync(Guid id)
        {
            var branch = await _db.Branches.Include(x=>x.Departments)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (branch is null)
                return null!; // handled by handler → Error.NotFound

            return _mapper.Map<GetBranchDto>(branch);
        }

        // ---------------- UPDATE ----------------
        public async Task<GetBranchDto> UpdateAsync(UpdateBranchDto dto)
        {
            var branch = await _db.Branches
                .Include(b => b.Departments)
                .FirstOrDefaultAsync(b => b.Id == dto.Id);

            if (branch is null) return null!;

            // Map branch fields except Departments
            branch.nameEn = dto.nameEn;
            branch.nameAr = dto.nameAr;
            branch.address = dto.address;
            branch.location = dto.location;

            // Handle departments
            var incomingDeptIds = dto.Departments.Select(d => d.Id).ToList();

            // Delete departments not in DTO
            var toDelete = branch.Departments.Where(d => !incomingDeptIds.Contains(d.Id)).ToList();
            foreach (var dept in toDelete)
            {
                _db.Departmentes.Remove(dept);
                branch.Departments.Remove(dept); // Remove from navigation property as well
            }

            // Update or add departments
            foreach (var deptDto in dto.Departments)
            {
                if (deptDto.Id == Guid.Empty)
                {
                    // New department
                    var newDept = _mapper.Map<Department>(deptDto);
                    newDept.BranchId = branch.Id;
                    await _db.Departmentes.AddAsync(newDept);
                    branch.Departments.Add(newDept); // Add to navigation property
                }
                else
                {
                    // Update existing
                    var existingDept = branch.Departments.FirstOrDefault(d => d.Id == deptDto.Id);
                    if (existingDept != null)
                    {
                        existingDept.nameEn = deptDto.nameEn;
                        existingDept.nameAr = deptDto.nameAr;
                        // Map other fields as needed
                    }
                }
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<GetBranchDto>(branch);
        }

    }
}
