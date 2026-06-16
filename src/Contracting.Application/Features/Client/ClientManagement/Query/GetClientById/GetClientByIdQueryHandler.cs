using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Query.GetClientById
{
    public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ErrorOr<GetClientDto>>
    {
        private readonly IClientService _service;

        public GetClientByIdQueryHandler(IClientService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetClientByIdAsync(request.ClientId);
            return result is null
                ? Error.NotFound("Client.NotFound", "Client not found.")
                : result;
        }
    }
}
