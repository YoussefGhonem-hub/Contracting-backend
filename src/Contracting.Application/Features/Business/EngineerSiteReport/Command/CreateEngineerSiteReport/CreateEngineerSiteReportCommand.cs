using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteReport
{
    public record CreateEngineerSiteReportCommand(CreateEngineerSiteReportDto Report) : IRequest<ErrorOr<GetEngineerSiteReportDto>>;
}
