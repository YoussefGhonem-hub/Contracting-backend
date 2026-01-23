using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportById
{
    public class GetEngineerSiteReportByIdQueryHandler : IRequestHandler<GetEngineerSiteReportByIdQuery, ErrorOr<GetEngineerSiteReportDto>>
    {
        private readonly IEngineerSiteReportService _service;

        public GetEngineerSiteReportByIdQueryHandler(IEngineerSiteReportService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetEngineerSiteReportDto>> Handle(GetEngineerSiteReportByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerSiteReportByIdAsync(request.ReportId);

            return result is null
                ? Error.NotFound("Engineer site report not found.")
                : result;
        }
    }
}
