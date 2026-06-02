using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.FinancialClearance.Command.DeleteFinancialClearance
{
    public record DeleteFinancialClearanceCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
}
