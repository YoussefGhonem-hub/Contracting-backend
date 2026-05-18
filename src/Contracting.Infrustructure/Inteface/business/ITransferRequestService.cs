using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using Contracting.Shared.Common;
using ErrorOr;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface ITransferRequestService
    {
        Task<ErrorOr<GetTransferRequestDto>> CreateAsync(CreateTransferRequestDto dto);
        Task<ErrorOr<GetTransferRequestDto>> UpdateAsync(UpdateTransferRequestDto dto);
        Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id);
        Task<ErrorOr<GetTransferRequestDto>> GetByIdAsync(Guid id);
        Task<PaginatedList<GetTransferRequestDto>> GetAllAsync(TransferRequestFilterDto filter);
        Task<ErrorOr<GetTransferRequestDto>> TakeActionAsync(Guid id, TransferRequestActionDto dto);
    }
}
