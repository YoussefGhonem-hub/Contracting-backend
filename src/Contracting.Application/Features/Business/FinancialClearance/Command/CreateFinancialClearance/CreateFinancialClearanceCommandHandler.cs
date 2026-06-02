using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.CreateFinancialClearance
{
    public class CreateFinancialClearanceCommandHandler : IRequestHandler<CreateFinancialClearanceCommand, ErrorOr<GetFinancialClearanceDto>>
    {
        private readonly IFinancialClearanceService _service;
        public CreateFinancialClearanceCommandHandler(IFinancialClearanceService service) => _service = service;
        public Task<ErrorOr<GetFinancialClearanceDto>> Handle(CreateFinancialClearanceCommand request, CancellationToken cancellationToken)
            => _service.CreateAsync(request.Dto);
    }
}
