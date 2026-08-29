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

            if (result.Success)
                return result.Data!;

            return result.FailureReason switch
            {
                RequestTypeDefaultDepartmentFailureReason.NotFound =>
                    Error.NotFound("RequestTypeDefaultDepartment.NotFound", "Config not found."),
                RequestTypeDefaultDepartmentFailureReason.InvalidDepartment =>
                    Error.Validation("RequestTypeDefaultDepartment.InvalidDepartment", "The department does not exist or does not belong to this branch."),
                _ => Error.Failure("RequestTypeDefaultDepartment.UpdateFailed", "Could not update the default department.")
            };
        }
    }
}
