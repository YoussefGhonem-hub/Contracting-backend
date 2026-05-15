using Contracting.Infrustructure.Inteface.business;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.ConfirmDeliveryDate
{
    public class ConfirmDeliveryDateCommandHandler : IRequestHandler<ConfirmDeliveryDateCommand, ErrorOr<bool>>
    {
        private readonly IEngineerRequestService _service;

        public ConfirmDeliveryDateCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(ConfirmDeliveryDateCommand request, CancellationToken cancellationToken)
        {
            return await _service.ConfirmDeliveryDateAsync(request.RequestId);
        }
    }
}
