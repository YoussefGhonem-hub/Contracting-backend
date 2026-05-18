using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Query.GetFinancialClearanceById
{
    public class GetFinancialClearanceByIdQueryHandler : IRequestHandler<GetFinancialClearanceByIdQuery, ErrorOr<GetFinancialClearanceDto>>
    {
        private readonly IFinancialClearanceService _service;
        public GetFinancialClearanceByIdQueryHandler(IFinancialClearanceService service) => _service = service;
        public Task<ErrorOr<GetFinancialClearanceDto>> Handle(GetFinancialClearanceByIdQuery request, CancellationToken cancellationToken)
            => _service.GetByIdAsync(request.Id);
    }
}
