using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest
{
    public class CreateEngineerRequestCommandHandler : IRequestHandler<CreateEngineerRequestCommand, ErrorOr<GetAllEngineerRequestDto>>
    {
        private readonly IEngineerRequestService _service;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public CreateEngineerRequestCommandHandler(IEngineerRequestService service, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetAllEngineerRequestDto>> Handle(CreateEngineerRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateEngineerRequestAsync(request.Request);
            
            return result is null
                ? Error.Forbidden(_localizer[SharedResourcesKeys.SiteEngineerOnlyCreateRequest])
                : result;
        }
    }
}