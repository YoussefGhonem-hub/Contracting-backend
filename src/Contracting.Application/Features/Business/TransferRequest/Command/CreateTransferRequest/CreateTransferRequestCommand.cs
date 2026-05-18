using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.CreateTransferRequest
{
    public record CreateTransferRequestCommand(CreateTransferRequestDto Dto) : IRequest<ErrorOr<GetTransferRequestDto>>;
}
