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
            var entity = _mapper.Map<Branch>(dto);

            await _db.Branches.AddAsync(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<GetBranchDto>(entity);
        }

        // ---------------- DELETE ----------------
        public async Task DeleteAsync(Guid id)
        {
            var branch = await _db.Branches.FindAsync(id);
            if (branch is null)
                return;

            _db.Branches.Remove(branch);
            await _db.SaveChangesAsync();
        }

        // ---------------- GET ALL (PAGINATION) ----------------
        public async Task<PaginatedList<GetBranchDto>> GetAllAsync(
            BaseFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Branches
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
            var branch = await _db.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (branch is null)
                return null!; // handled by handler → Error.NotFound

            return _mapper.Map<GetBranchDto>(branch);
        }

        // ---------------- UPDATE ----------------
        public async Task<GetBranchDto> UpdateAsync(UpdateBranchDto dto)
        {
            var entity = await _db.Branches.FindAsync(dto.Id);

            if (entity is null)
                Error.

            // Map only updated fields
            _mapper.Map(dto, entity);

            await _db.SaveChangesAsync();

            return _mapper.Map<GetBranchDto>(entity);
        }
    }
}
