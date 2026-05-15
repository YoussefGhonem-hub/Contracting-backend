using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.ConfirmDeliveryDate
{
    public record ConfirmDeliveryDateCommand(Guid RequestId) : IRequest<ErrorOr<bool>>;
}
