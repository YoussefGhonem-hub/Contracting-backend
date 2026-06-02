using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.UpdateFinancialClearance
{
    public class UpdateFinancialClearanceCommandHandler : IRequestHandler<UpdateFinancialClearanceCommand, ErrorOr<GetFinancialClearanceDto>>
    {
        private readonly IFinancialClearanceService _service;
        public UpdateFinancialClearanceCommandHandler(IFinancialClearanceService service) => _service = service;
        public Task<ErrorOr<GetFinancialClearanceDto>> Handle(UpdateFinancialClearanceCommand request, CancellationToken cancellationToken)
            => _service.UpdateAsync(request.Dto);
    }
}
