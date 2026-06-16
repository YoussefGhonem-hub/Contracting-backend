using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.ReassignFinancialClearance
{
    public record ReassignFinancialClearanceCommand(Guid Id, ReassignFinancialClearanceDto Dto) : IRequest<ErrorOr<GetFinancialClearanceDto>>;
}
