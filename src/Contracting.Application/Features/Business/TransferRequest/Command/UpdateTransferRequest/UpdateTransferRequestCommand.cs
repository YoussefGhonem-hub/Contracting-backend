using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.UpdateTransferRequest
{
    public record UpdateTransferRequestCommand(UpdateTransferRequestDto Dto) : IRequest<ErrorOr<GetTransferRequestDto>>;
}
