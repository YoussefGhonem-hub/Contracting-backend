using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Branch.Command.DeleteBranch
{
    public sealed class DeleteBranchCommandHandler
        : IRequestHandler<DeleteBranchCommand, ErrorOr<GenericResponse>>
    {
        private readonly IBranchService _service;

        public DeleteBranchCommandHandler(IBranchService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteAsync(request.Id);
            return result.Success
                ? result
                : Error.Failure(result.Message ?? "Failed to delete branch");
            
        }
    }
}