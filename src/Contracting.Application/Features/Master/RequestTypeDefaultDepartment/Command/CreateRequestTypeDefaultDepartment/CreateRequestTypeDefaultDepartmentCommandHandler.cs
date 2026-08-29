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

            if (result.Success)
                return result.Data!;

            return result.FailureReason switch
            {
                RequestTypeDefaultDepartmentFailureReason.AlreadyExists =>
                    Error.Conflict("RequestTypeDefaultDepartment.AlreadyExists",
                        $"A default department is already configured for '{request.Dto.RequestType}' on this branch: " +
                        $"'{result.ExistingConfig!.DepartmentNameEn}' (departmentId: {result.ExistingConfig.DepartmentId}, configId: {result.ExistingConfig.Id}). " +
                        "Use PUT /api/RequestTypeDefaultDepartment with that configId to change it instead of creating a new one."),
                RequestTypeDefaultDepartmentFailureReason.InvalidDepartment =>
                    Error.Validation("RequestTypeDefaultDepartment.InvalidDepartment", "The department does not exist or does not belong to this branch."),
                _ => Error.Failure("RequestTypeDefaultDepartment.CreateFailed", "Could not create the default department.")
            };
        }
    }
}
