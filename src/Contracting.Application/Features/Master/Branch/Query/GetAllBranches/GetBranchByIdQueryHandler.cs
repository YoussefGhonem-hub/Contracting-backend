using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Query.GetAllBranches
{
    public sealed class GetBranchByIdQueryHandler
    : IRequestHandler<GetBranchByIdQuery, ErrorOr<GetBranchDto>>
    {
        private readonly IBranchService _service;

        public GetBranchByIdQueryHandler(IBranchService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetBranchDto>> Handle(
            GetBranchByIdQuery request,
            CancellationToken cancellationToken)
        {
            var branch = await _service.GetByIdAsync(request.Id);

            return branch is null
                ? Error.NotFound("Branch not found")
                : branch;
        }
    }
}
