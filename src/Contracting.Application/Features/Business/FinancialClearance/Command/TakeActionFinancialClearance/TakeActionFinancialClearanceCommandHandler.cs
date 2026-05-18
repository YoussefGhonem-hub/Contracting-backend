using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.TakeActionFinancialClearance
{
    public class TakeActionFinancialClearanceCommandHandler : IRequestHandler<TakeActionFinancialClearanceCommand, ErrorOr<GetFinancialClearanceDto>>
    {
        private readonly IFinancialClearanceService _service;
        public TakeActionFinancialClearanceCommandHandler(IFinancialClearanceService service) => _service = service;
        public Task<ErrorOr<GetFinancialClearanceDto>> Handle(TakeActionFinancialClearanceCommand request, CancellationToken cancellationToken)
            => _service.TakeActionAsync(request.Id, request.ActionDto);
    }
}
