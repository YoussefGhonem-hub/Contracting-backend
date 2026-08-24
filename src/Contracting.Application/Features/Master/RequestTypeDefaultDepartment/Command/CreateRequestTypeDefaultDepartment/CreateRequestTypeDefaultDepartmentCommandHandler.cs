using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.CreateRequestTypeDefaultDepartment
{
    public class CreateRequestTypeDefaultDepartmentCommandHandler : IRequestHandler<CreateRequestTypeDefaultDepartmentCommand, ErrorOr<GetRequestTypeDefaultDepartmentDto>>
    {
        private readonly IRequestTypeDefaultDepartmentService _service;

        public CreateRequestTypeDefaultDepartmentCommandHandler(IRequestTypeDefaultDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetRequestTypeDefaultDepartmentDto>> Handle(CreateRequestTypeDefaultDepartmentCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request.Dto);

            return result is null
                ? Error.Failure("RequestTypeDefaultDepartment.CreateFailed", "Could not create the default department — the department may not exist in this branch, or a default is already configured for this branch/request type.")
                : result;
        }
    }
}
