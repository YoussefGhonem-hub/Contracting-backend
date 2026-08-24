using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.CreateRequestTypeDefaultDepartment
{
    public record CreateRequestTypeDefaultDepartmentCommand(CreateRequestTypeDefaultDepartmentDto Dto) : IRequest<ErrorOr<GetRequestTypeDefaultDepartmentDto>>;
}
