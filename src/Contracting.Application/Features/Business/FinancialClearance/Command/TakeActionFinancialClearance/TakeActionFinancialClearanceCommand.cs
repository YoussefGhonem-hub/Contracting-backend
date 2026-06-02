using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.TakeActionFinancialClearance
{
    public record TakeActionFinancialClearanceCommand(Guid Id, FinancialClearanceActionDto ActionDto) : IRequest<ErrorOr<GetFinancialClearanceDto>>;
}
