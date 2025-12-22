using Contracting.Infrustructure.Inteface;
using ErrorOr;
using MediatR;

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

        public async Task<ErrorOr<bool>> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _service.GetByIdAsync(request.Id);
            if (branch is null)
                return Error.NotFound(description: "Branch not found.");

            try
            {
                await _service.DeleteAsync(request.Id);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                return Error.Failure(description: ex.Message); // Localize if needed
            }
        }
    }
}