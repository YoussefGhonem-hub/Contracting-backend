using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Query.GetAllFinancialClearances
{
    public record GetAllFinancialClearancesQuery(FinancialClearanceFilterDto Filter) : IRequest<PaginatedList<GetFinancialClearanceDto>>;
}
