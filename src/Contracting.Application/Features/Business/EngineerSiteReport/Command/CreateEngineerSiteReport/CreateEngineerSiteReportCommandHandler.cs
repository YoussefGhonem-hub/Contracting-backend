using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteReport
{
    public class CreateEngineerSiteReportCommandHandler : IRequestHandler<CreateEngineerSiteReportCommand, ErrorOr<GetEngineerSiteReportDto>>
    {
        private readonly IEngineerSiteReportService _service;

        public CreateEngineerSiteReportCommandHandler(IEngineerSiteReportService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetEngineerSiteReportDto>> Handle(CreateEngineerSiteReportCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateEngineerSiteReportAsync(request.Report);

            return result is null
                ? Error.Failure("Could not create engineer site report.")
                : result;
        }
    }
}
