using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestById
{
    public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, ErrorOr<GetAllEngineerRequestDto>>
    {
        private readonly IEngineerRequestService _service;

        public GetRequestByIdQueryHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetAllEngineerRequestDto>> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerRequestByIdAsync(request.RequestId);
            
            return result is null
                ? Error.NotFound("Engineer request not found.")
                : result;
        }
    }
}