using Contracting.Shared.Dtos.ClientDtos.ReportDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.SiteReport.Query.GetClientSiteReports;

public record GetClientSiteReportsQuery(Guid ProjectId) : IRequest<ErrorOr<List<GetClientSiteReportListItemDto>>>;
