using Contracting.Shared.Resources;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Contracting.Shared.Dtos.MasterDtos.BranchDto;

namespace Contracting.Infrustructure.Features
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public BranchService(ApplicationDbContext db, IMapper mapper, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _mapper = mapper;
            _localizer = localizer;
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
        public async Task<GenericResponse> DeleteAsync(Guid id)
        {
            var branch = await _db.Branches.FindAsync(id);
            if (branch is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.BranchNotFound]);
            
            // Check for related departments
            bool hasDepartments = await _db.Departmentes.AnyAsync(d => d.Branch.Id == id);
            if (hasDepartments)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.BranchHasDepartments]);

            _db.Branches.Remove(branch);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.BranchDeleteSuccess]);
        }

        public async Task<List<BranchDropDownDto>> DropDownMethodAsync()
        {
            return await _db.Branches
                        .Select(b => new BranchDropDownDto
                        {
                            Id = b.Id,
                            nameAr = b.nameAr,
                            nameEn = b.nameEn
,                        })
                        .ToListAsync();
        }

        // ---------------- GET ALL (PAGINATION) ----------------
        public async Task<PaginatedList<GetBranchDto>> GetAllAsync(
            BaseFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Branches
               .Include(b => b.Departments)
               .AsNoTracking();

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderBy(b => b.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and materialize the data
            var branches = await query
                .Skip((filter.PageIndex -1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs after materialization (not in LINQ projection)
            var branchDtos = _mapper.Map<List<GetBranchDto>>(branches);

            // Return paginated result
            return new PaginatedList<GetBranchDto>(
                branchDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
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
                    }
                }
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<GetBranchDto>(branch);
        }

    }
}
