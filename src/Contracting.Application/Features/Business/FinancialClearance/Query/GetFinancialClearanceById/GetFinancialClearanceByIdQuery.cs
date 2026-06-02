using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Query.GetFinancialClearanceById
{
    public record GetFinancialClearanceByIdQuery(Guid Id) : IRequest<ErrorOr<GetFinancialClearanceDto>>;
}
