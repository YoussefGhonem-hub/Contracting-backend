using Contracting.Application.Features.Master.Branch.Query.GetAllBranches;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Branch.Query.GetBranchById
{
    public sealed class GetAllBranchesQueryHandler
    : IRequestHandler<GetAllBranchesQuery, ErrorOr<PaginatedList<GetBranchDto>>>
    {
        private readonly IBranchService _service;

        public GetAllBranchesQueryHandler(IBranchService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetBranchDto>>> Handle(
            GetAllBranchesQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
