using Contracting.Infrustructure.Inteface;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.DeleteBranch
{
    public sealed class DeleteBranchCommandHandler
    : IRequestHandler<DeleteBranchCommand, ErrorOr<bool>>
    {
        private readonly IBranchService _service;

        public DeleteBranchCommandHandler(IBranchService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(
            DeleteBranchCommand request,
            CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(request.Id);
            return true;
        }
    }
}
