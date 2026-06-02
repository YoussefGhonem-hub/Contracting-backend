using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.TakeActionTransferRequest
{
    public record TakeActionTransferRequestCommand(Guid Id, TransferRequestActionDto ActionDto) : IRequest<ErrorOr<GetTransferRequestDto>>;
}
