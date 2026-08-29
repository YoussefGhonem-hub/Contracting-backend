using Contracting.Shared.Common;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;

namespace Contracting.Infrustructure.Inteface
{
    public enum RequestTypeDefaultDepartmentFailureReason
    {
        None,
        NotFound,
        InvalidDepartment,
        AlreadyExists
    }

    public class RequestTypeDefaultDepartmentResult
    {
        public GetRequestTypeDefaultDepartmentDto? Data { get; init; }
        public RequestTypeDefaultDepartmentFailureReason FailureReason { get; init; } = RequestTypeDefaultDepartmentFailureReason.None;
        public bool Success => Data is not null;

        /// <summary>Set only when FailureReason is AlreadyExists — the config that's already there, so the caller can show what it's currently pointing to.</summary>
        public GetRequestTypeDefaultDepartmentDto? ExistingConfig { get; init; }

        public static RequestTypeDefaultDepartmentResult Ok(GetRequestTypeDefaultDepartmentDto data) => new() { Data = data };
        public static RequestTypeDefaultDepartmentResult Fail(RequestTypeDefaultDepartmentFailureReason reason) => new() { FailureReason = reason };
        public static RequestTypeDefaultDepartmentResult Conflict(GetRequestTypeDefaultDepartmentDto existing) =>
            new() { FailureReason = RequestTypeDefaultDepartmentFailureReason.AlreadyExists, ExistingConfig = existing };
    }

    public interface IRequestTypeDefaultDepartmentService
    {
        Task<RequestTypeDefaultDepartmentResult> CreateAsync(CreateRequestTypeDefaultDepartmentDto dto);

        Task<RequestTypeDefaultDepartmentResult> UpdateAsync(UpdateRequestTypeDefaultDepartmentDto dto);

        Task<GenericResponse> DeleteAsync(Guid id);

        Task<List<GetRequestTypeDefaultDepartmentDto>> GetAllByBranchAsync(Guid branchId);

        Task<GetDefaultDepartmentDto> GetDefaultAsync(Guid branchId, string requestType);
    }
}
