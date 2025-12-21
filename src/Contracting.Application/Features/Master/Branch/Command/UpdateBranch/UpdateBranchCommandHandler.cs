using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.UpdateBranch
{
    public sealed class UpdateBranchCommandHandler
    : IRequestHandler<UpdateBranchCommand, ErrorOr<GetBranchDto>>
    {
        private readonly IBranchService _service;

        public UpdateBranchCommandHandler(IBranchService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetBranchDto>> Handle(
            UpdateBranchCommand request,
            CancellationToken cancellationToken)
        {
            var obj = await _service.UpdateAsync(request.Branch);
            return obj;
        }
    }
}
