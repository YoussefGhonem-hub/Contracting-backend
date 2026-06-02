using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.CreateFinancialClearance
{
    public record CreateFinancialClearanceCommand(CreateFinancialClearanceDto Dto) : IRequest<ErrorOr<GetFinancialClearanceDto>>;
}
