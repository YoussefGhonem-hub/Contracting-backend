using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Query.GetAllFinancialClearances
{
    public class GetAllFinancialClearancesQueryHandler : IRequestHandler<GetAllFinancialClearancesQuery, PaginatedList<GetFinancialClearanceDto>>
    {
        private readonly IFinancialClearanceService _service;
        public GetAllFinancialClearancesQueryHandler(IFinancialClearanceService service) => _service = service;
        public Task<PaginatedList<GetFinancialClearanceDto>> Handle(GetAllFinancialClearancesQuery request, CancellationToken cancellationToken)
            => _service.GetAllAsync(request.Filter);
    }
}
