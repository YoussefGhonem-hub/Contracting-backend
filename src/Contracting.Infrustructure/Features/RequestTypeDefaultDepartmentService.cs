using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using Contracting.Shared.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Contracting.Infrustructure.Features
{
    public class RequestTypeDefaultDepartmentService : IRequestTypeDefaultDepartmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public RequestTypeDefaultDepartmentService(ApplicationDbContext db, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _localizer = localizer;
        }

        public async Task<GetRequestTypeDefaultDepartmentDto?> CreateAsync(CreateRequestTypeDefaultDepartmentDto dto)
        {
            var department = await _db.Departmentes.FirstOrDefaultAsync(d => d.Id == dto.DepartmentId);
            if (department is null || department.BranchId != dto.BranchId)
                return null;

            var alreadyConfigured = await _db.RequestTypeDefaultDepartments
                .AnyAsync(x => x.BranchId == dto.BranchId && x.RequestType == dto.RequestType);
            if (alreadyConfigured)
                return null;

            var entity = new RequestTypeDefaultDepartment
            {
                BranchId = dto.BranchId,
                RequestType = dto.RequestType,
                DepartmentId = dto.DepartmentId
            };

            await _db.RequestTypeDefaultDepartments.AddAsync(entity);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<GetRequestTypeDefaultDepartmentDto?> UpdateAsync(UpdateRequestTypeDefaultDepartmentDto dto)
        {
            var entity = await _db.RequestTypeDefaultDepartments.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (entity is null)
                return null;

            var department = await _db.Departmentes.FirstOrDefaultAsync(d => d.Id == dto.DepartmentId);
            if (department is null || department.BranchId != entity.BranchId)
                return null;

            entity.DepartmentId = dto.DepartmentId;
            await _db.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        public async Task<GenericResponse> DeleteAsync(Guid id)
        {
            var entity = await _db.RequestTypeDefaultDepartments.FindAsync(id);
            if (entity is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.NotFound]);

            _db.RequestTypeDefaultDepartments.Remove(entity);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.DeleteSuccess]);
        }

        public async Task<List<GetRequestTypeDefaultDepartmentDto>> GetAllByBranchAsync(Guid branchId)
        {
            return await _db.RequestTypeDefaultDepartments
                .Where(x => x.BranchId == branchId)
                .Include(x => x.Department)
                .AsNoTracking()
                .Select(x => new GetRequestTypeDefaultDepartmentDto
                {
                    Id = x.Id,
                    BranchId = x.BranchId,
                    RequestType = x.RequestType,
                    DepartmentId = x.DepartmentId,
                    DepartmentNameEn = x.Department.nameEn,
                    DepartmentNameAr = x.Department.nameAr
                })
                .ToListAsync();
        }

        public async Task<GetDefaultDepartmentDto> GetDefaultAsync(Guid branchId, string requestType)
        {
            var config = await _db.RequestTypeDefaultDepartments
                .Include(x => x.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BranchId == branchId && x.RequestType == requestType);

            if (config is null)
                return new GetDefaultDepartmentDto { HasDefault = false };

            return new GetDefaultDepartmentDto
            {
                HasDefault = true,
                DepartmentId = config.DepartmentId,
                DepartmentNameEn = config.Department.nameEn,
                DepartmentNameAr = config.Department.nameAr
            };
        }

        private async Task<GetRequestTypeDefaultDepartmentDto?> GetByIdAsync(Guid id)
        {
            return await _db.RequestTypeDefaultDepartments
                .Where(x => x.Id == id)
                .Include(x => x.Department)
                .AsNoTracking()
                .Select(x => new GetRequestTypeDefaultDepartmentDto
                {
                    Id = x.Id,
                    BranchId = x.BranchId,
                    RequestType = x.RequestType,
                    DepartmentId = x.DepartmentId,
                    DepartmentNameEn = x.Department.nameEn,
                    DepartmentNameAr = x.Department.nameAr
                })
                .FirstOrDefaultAsync();
        }
    }
}
