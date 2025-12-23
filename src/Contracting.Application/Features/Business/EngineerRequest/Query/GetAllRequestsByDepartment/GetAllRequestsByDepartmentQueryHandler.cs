using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAllRequestsByDepartment
{
    public class GetAllRequestsByDepartmentQueryHandler : IRequestHandler<GetAllRequestsByDepartmentQuery, ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetAllRequestsByDepartmentQueryHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>> Handle(GetAllRequestsByDepartmentQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllEngineerRequestsByDepartmentAsync(
                request.DepartmentId, 
                request.Filter, 
                cancellationToken);
            
            return result;
        }
    }
}