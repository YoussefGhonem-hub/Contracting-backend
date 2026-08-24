using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.UpdateRequestTypeDefaultDepartment
{
    public class UpdateRequestTypeDefaultDepartmentCommandHandler : IRequestHandler<UpdateRequestTypeDefaultDepartmentCommand, ErrorOr<GetRequestTypeDefaultDepartmentDto>>
    {
        private readonly IRequestTypeDefaultDepartmentService _service;

        public UpdateRequestTypeDefaultDepartmentCommandHandler(IRequestTypeDefaultDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetRequestTypeDefaultDepartmentDto>> Handle(UpdateRequestTypeDefaultDepartmentCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(request.Dto);

            return result is null
                ? Error.NotFound("RequestTypeDefaultDepartment.NotFound", "Config not found, or the department does not belong to this branch.")
                : result;
        }
    }
}
