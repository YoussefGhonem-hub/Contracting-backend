using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Query.GetTransferRequestById
{
    public record GetTransferRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GetTransferRequestDto>>;
}
