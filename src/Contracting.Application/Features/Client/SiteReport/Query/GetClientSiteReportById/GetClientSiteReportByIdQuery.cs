using Contracting.Shared.Dtos.ClientDtos.ReportDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.SiteReport.Query.GetClientSiteReportById;

public record GetClientSiteReportByIdQuery(Guid ReportId) : IRequest<ErrorOr<GetClientSiteReportDetailDto>>;
