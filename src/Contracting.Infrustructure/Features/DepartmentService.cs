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
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;

namespace Contracting.Infrustructure.Features
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public DepartmentService(ApplicationDbContext db, IMapper mapper, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<GetDepartmentDto> CreateDepartmentAsync(Guid branchId, CreateDepartmentDto departmentDto)
        {
            var branch = await _db.Branches.FindAsync(branchId);
            if (branch is null)
                return null!;

            var department = _mapper.Map<Department>(departmentDto);
            department.BranchId = branchId;
            department.hasSpecialFields = departmentDto.hasSpecialFields;
            department.RequiresGoodsReceipt = departmentDto.RequiresGoodsReceipt;

            await _db.Departmentes.AddAsync(department);
            await _db.SaveChangesAsync();

            if (departmentDto.hasSpecialFields && departmentDto.SpecialFields.Any())
            {
                await AddDepartmentSpecialFieldsAsync(department, departmentDto.SpecialFields);
            }

            return await GetDepartmentByIdAsync(department.Id);
        }

        public async Task<GetDepartmentDto> UpdateDepartmentAsync(UpdateDepartmentDto departmentDto)
        {
            var department = await _db.Departmentes
                .Include(d => d.DepartmentSpecialFields)
                    .ThenInclude(dsf => dsf.SpecialField)
                .FirstOrDefaultAsync(d => d.Id == departmentDto.Id);

            if (department is null)
                return null!;

            department.nameEn = departmentDto.nameEn;
            department.nameAr = departmentDto.nameAr;
            department.hasSpecialFields = departmentDto.hasSpecialFields;
            department.RequiresGoodsReceipt = departmentDto.RequiresGoodsReceipt;

            await _db.SaveChangesAsync();

            await ReplaceDepartmentSpecialFieldsAsync(department, departmentDto.SpecialFields);

            return await GetDepartmentByIdAsync(department.Id);
        }

        public async Task<PaginatedList<GetDepartmentDto>> GetDepartmentsByBranchIdAsync(
            Guid branchId,
            BaseFilterDto filter,
            CancellationToken cancellationToken)
        {
            try
            {
                var query = _db.Departmentes
                    .Include(d => d.DepartmentSpecialFields)
                        .ThenInclude(dsf => dsf.SpecialField)
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

                if (totalCount == 0)
                {
                    return new PaginatedList<GetDepartmentDto>(
                        new List<GetDepartmentDto>(),
                        0,
                        filter.PageIndex,
                        filter.PageSize);
                }

                // Apply pagination and materialize the data
                var departments = await query
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync(cancellationToken);

                // Map to DTOs after materialization (not in LINQ projection)
                var departmentDtos = _mapper.Map<List<GetDepartmentDto>>(departments);

                for (var index = 0; index < departments.Count; index++)
                {
                    var specialFields = departments[index].DepartmentSpecialFields
                        .OrderBy(dsf => dsf.Order)
                        .Select(dsf => new DepartmentSpecialFieldDto
                        {
                            Id = dsf.Id,
                            SpecialFieldId = dsf.SpecialFieldId,
                            name = dsf.SpecialField?.name,
                            fieldType = dsf.SpecialField?.fieldType,
                            value = dsf.value,
                            Order = dsf.Order,
                            ColSpan = dsf.ColSpan
                        })
                        .ToList();

                    departmentDtos[index].SpecialFields = specialFields;
                    departmentDtos[index].hasSpecialFields = specialFields.Any();
                }

                // Return paginated result
                return new PaginatedList<GetDepartmentDto>(
                    departmentDtos,
                    totalCount,
                    filter.PageIndex,
                    filter.PageSize);
            }
            catch (Exception)
            {
                return new PaginatedList<GetDepartmentDto>(
                    new List<GetDepartmentDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }
        }

        public async Task<GenericResponse> RemoveDepartmentAsync(Guid branchId, Guid departmentId)
        {
            var department = await _db.Departmentes
                .FirstOrDefaultAsync(d => d.Id == departmentId && d.BranchId == branchId);

            if (department is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.DepartmentNotFound]);

            _db.Departmentes.Remove(department);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.DepartmentRemoveSuccess]);
        }

        public async Task<List<GetDepartmentDto>> DropDownMethodAsync(Guid branchId)
        {
            var departments = await _db.Departmentes
                   .Include(d => d.DepartmentSpecialFields)
                       .ThenInclude(dsf => dsf.SpecialField)
                   .Where(x => x.BranchId == branchId)
                   .AsNoTracking()
                   .ToListAsync();

            var dtos = departments.Select(x => new GetDepartmentDto
            {
                Id = x.Id,
                nameAr = x.nameAr,
                nameEn = x.nameEn,
                hasSpecialFields = x.hasSpecialFields || x.DepartmentSpecialFields.Any(),
                RequiresGoodsReceipt = x.RequiresGoodsReceipt,
                SpecialFields = x.DepartmentSpecialFields.OrderBy(dsf => dsf.Order).Select(dsf => new DepartmentSpecialFieldDto
                {
                    Id = dsf.Id,
                    SpecialFieldId = dsf.SpecialFieldId,
                    name = dsf.SpecialField?.name,
                    fieldType = dsf.SpecialField?.fieldType,
                    value = dsf.value,
                    Order = dsf.Order,
                    ColSpan = dsf.ColSpan
                }).ToList()
            }).ToList();

            return dtos;
        }

        public async Task<DepartmentSpecialFieldsCheckDto> GetDepartmentSpecialFieldsAsync(Guid departmentId)
        {
            var department = await _db.Departmentes
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department is null)
                return null!;

            var allDepartmentSpecialFields = await _db.DepartmentSpecialFields
                .IgnoreQueryFilters()
                .Where(dsf => dsf.DepartmentId == departmentId)
                .Include(dsf => dsf.SpecialField)
                .AsNoTracking()
                .ToListAsync();

            var activeDepartmentSpecialFields = allDepartmentSpecialFields
                .Where(dsf => !dsf.IsDeleted && (dsf.SpecialField == null || !dsf.SpecialField.IsDeleted))
                .ToList();

            var departmentSpecialFieldsToReturn = activeDepartmentSpecialFields.Any()
                ? activeDepartmentSpecialFields
                : allDepartmentSpecialFields;

            var specialFields = departmentSpecialFieldsToReturn
                .OrderBy(dsf => dsf.Order)
                .Select(dsf => new DepartmentSpecialFieldDto
                {
                    Id = dsf.Id,
                    SpecialFieldId = dsf.SpecialFieldId,
                    name = dsf.SpecialField?.name,
                    fieldType = dsf.SpecialField?.fieldType,
                    value = dsf.value,
                    Order = dsf.Order,
                    ColSpan = dsf.ColSpan
                })
                .ToList();
            var hasSpecialFields = specialFields.Any();

            return new DepartmentSpecialFieldsCheckDto
            {
                DepartmentId = department.Id,
                hasSpecialFields = hasSpecialFields,
                SpecialFields = hasSpecialFields
                    ? specialFields
                    : new List<DepartmentSpecialFieldDto>()
            };
        }

        private async Task<GetDepartmentDto> GetDepartmentByIdAsync(Guid departmentId)
        {
            var department = await _db.Departmentes
                .Include(d => d.Branch)
                .Include(d => d.DepartmentSpecialFields)
                    .ThenInclude(dsf => dsf.SpecialField)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department is null)
                return null!;

            var dto = _mapper.Map<GetDepartmentDto>(department);
            var specialFields = department.DepartmentSpecialFields
                .OrderBy(dsf => dsf.Order)
                .Select(dsf => new DepartmentSpecialFieldDto
                {
                    Id = dsf.Id,
                    SpecialFieldId = dsf.SpecialFieldId,
                    name = dsf.SpecialField?.name,
                    fieldType = dsf.SpecialField?.fieldType,
                    value = dsf.value,
                    Order = dsf.Order,
                    ColSpan = dsf.ColSpan
                })
                .ToList();

            dto.SpecialFields = specialFields;
            dto.hasSpecialFields = specialFields.Any();

            return dto;
        }

        private async Task AddDepartmentSpecialFieldsAsync(Department department, List<CreateDepartmentSpecialFieldDto> fields)
        {
            if (fields is null || fields.Count == 0)
            {
                return;
            }

            var departmentFields = new List<DepartmentSpecialField>();

            for (var i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var specialField = new SpecialField
                {
                    name = field.name,
                    fieldType = field.fieldType
                };

                departmentFields.Add(new DepartmentSpecialField
                {
                    DepartmentId = department.Id,
                    SpecialField = specialField,
                    value = field.value,
                    Order = field.Order != 0 ? field.Order : i,
                    ColSpan = field.ColSpan is >= 1 and <= 4 ? field.ColSpan : 1
                });
            }

            await _db.DepartmentSpecialFields.AddRangeAsync(departmentFields);
            await _db.SaveChangesAsync();
        }

        private async Task ReplaceDepartmentSpecialFieldsAsync(Department department, List<CreateDepartmentSpecialFieldDto> fields)
        {
            if (!department.hasSpecialFields)
            {
                var toRemove = await _db.DepartmentSpecialFields
                    .Include(dsf => dsf.SpecialField)
                    .Where(dsf => dsf.DepartmentId == department.Id)
                    .ToListAsync();

                if (toRemove.Any())
                {
                    _db.DepartmentSpecialFields.RemoveRange(toRemove);
                    var specials = toRemove.Select(e => e.SpecialField).Where(sf => sf != null).ToList();
                    if (specials.Any())
                    {
                        _db.SpecialFields.RemoveRange(specials);
                    }

                    await _db.SaveChangesAsync();
                }

                return;
            }

            if (fields is null || fields.Count == 0)
            {
                return;
            }

            var existing = await _db.DepartmentSpecialFields
                .Include(dsf => dsf.SpecialField)
                .Where(dsf => dsf.DepartmentId == department.Id)
                .ToListAsync();

            if (existing.Any())
            {
                _db.DepartmentSpecialFields.RemoveRange(existing);
                var specials = existing.Select(e => e.SpecialField).Where(sf => sf != null).ToList();
                if (specials.Any())
                {
                    _db.SpecialFields.RemoveRange(specials);
                }

                await _db.SaveChangesAsync();
            }

            await AddDepartmentSpecialFieldsAsync(department, fields);
        }
    }

}
