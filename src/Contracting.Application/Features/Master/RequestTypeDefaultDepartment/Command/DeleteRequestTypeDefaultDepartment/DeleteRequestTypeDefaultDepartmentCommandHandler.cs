using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.DeleteRequestTypeDefaultDepartment
{
    public class DeleteRequestTypeDefaultDepartmentCommandHandler : IRequestHandler<DeleteRequestTypeDefaultDepartmentCommand, ErrorOr<GenericResponse>>
    {
        private readonly IRequestTypeDefaultDepartmentService _service;

        public DeleteRequestTypeDefaultDepartmentCommandHandler(IRequestTypeDefaultDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteRequestTypeDefaultDepartmentCommand request, CancellationToken cancellationToken)
        {
            return await _service.DeleteAsync(request.Id);
        }
    }
}
