using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.UpdateRequestTypeDefaultDepartment
{
    public record UpdateRequestTypeDefaultDepartmentCommand(UpdateRequestTypeDefaultDepartmentDto Dto) : IRequest<ErrorOr<GetRequestTypeDefaultDepartmentDto>>;
}
