using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Command.DeleteClient
{
    public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, ErrorOr<GenericResponse>>
    {
        private readonly IClientService _service;

        public DeleteClientCommandHandler(IClientService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteClientAsync(request.ClientId);
            return result.Success
                ? result
                : Error.NotFound("Client.NotFound", result.Message ?? "Client not found.");
        }
    }
}
