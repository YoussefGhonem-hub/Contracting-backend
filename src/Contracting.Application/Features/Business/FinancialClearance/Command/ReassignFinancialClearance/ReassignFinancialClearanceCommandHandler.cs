using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.ReassignFinancialClearance
{
    public class ReassignFinancialClearanceCommandHandler : IRequestHandler<ReassignFinancialClearanceCommand, ErrorOr<GetFinancialClearanceDto>>
    {
        private readonly IFinancialClearanceService _service;
        public ReassignFinancialClearanceCommandHandler(IFinancialClearanceService service) => _service = service;

        public Task<ErrorOr<GetFinancialClearanceDto>> Handle(ReassignFinancialClearanceCommand request, CancellationToken cancellationToken)
            => _service.ReassignAsync(request.Id, request.Dto);
    }
}
