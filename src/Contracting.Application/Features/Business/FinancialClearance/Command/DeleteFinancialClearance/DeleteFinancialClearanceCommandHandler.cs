using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.DeleteFinancialClearance
{
    public class DeleteFinancialClearanceCommandHandler : IRequestHandler<DeleteFinancialClearanceCommand, ErrorOr<GenericResponse>>
    {
        private readonly IFinancialClearanceService _service;
        public DeleteFinancialClearanceCommandHandler(IFinancialClearanceService service) => _service = service;
        public Task<ErrorOr<GenericResponse>> Handle(DeleteFinancialClearanceCommand request, CancellationToken cancellationToken)
            => _service.DeleteAsync(request.Id);
    }
}
