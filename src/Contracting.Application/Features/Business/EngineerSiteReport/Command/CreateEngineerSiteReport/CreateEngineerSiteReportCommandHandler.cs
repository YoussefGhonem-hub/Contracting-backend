using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteReport
{
    public class CreateEngineerSiteReportCommandHandler : IRequestHandler<CreateEngineerSiteReportCommand, ErrorOr<GetEngineerSiteReportDto>>
    {
        private readonly IEngineerSiteReportService _service;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public CreateEngineerSiteReportCommandHandler(IEngineerSiteReportService service, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerSiteReportDto>> Handle(CreateEngineerSiteReportCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateEngineerSiteReportAsync(request.Report);

            return result is null
                ? Error.Failure(_localizer[SharedResourcesKeys.EngineerSiteReportCreateFailed])
                : result;
        }
    }
}
