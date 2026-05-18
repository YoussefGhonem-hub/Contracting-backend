using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.DeleteTransferRequest
{
    public record DeleteTransferRequestCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
}
