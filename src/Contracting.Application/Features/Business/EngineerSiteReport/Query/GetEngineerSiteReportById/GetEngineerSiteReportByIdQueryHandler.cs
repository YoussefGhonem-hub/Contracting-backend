using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportById
{
    public class GetEngineerSiteReportByIdQueryHandler : IRequestHandler<GetEngineerSiteReportByIdQuery, ErrorOr<GetEngineerSiteReportDto>>
    {
        private readonly IEngineerSiteReportService _service;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public GetEngineerSiteReportByIdQueryHandler(IEngineerSiteReportService service, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerSiteReportDto>> Handle(GetEngineerSiteReportByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerSiteReportByIdAsync(request.ReportId);

            return result is null
                ? Error.NotFound(_localizer[SharedResourcesKeys.EngineerSiteReportNotFound])
                : result;
        }
    }
}
