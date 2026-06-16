using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Query.GetClientList
{
    public class GetClientListQueryHandler : IRequestHandler<GetClientListQuery, ErrorOr<PaginatedList<GetClientDto>>>
    {
        private readonly IClientService _service;

        public GetClientListQueryHandler(IClientService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetClientDto>>> Handle(GetClientListQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetClientListAsync(request.Filter);
        }
    }
}
