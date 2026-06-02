using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.UpdateFinancialClearance
{
    public record UpdateFinancialClearanceCommand(UpdateFinancialClearanceDto Dto) : IRequest<ErrorOr<GetFinancialClearanceDto>>;
}
