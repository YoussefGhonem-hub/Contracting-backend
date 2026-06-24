using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Contracting.Application.Features.Business.ClientContent.Command.UpdateMonthlyReport;

public record UpdateMonthlyReportCommand(
    Guid ReportId,
    int? Month,
    int? Year,
    string? Title,
    string? WorkProgress,
    ICollection<IFormFile>? Attachments,
    List<Guid>? RemoveAttachmentIds) : IRequest<ErrorOr<UpdatedMonthlyReportDto>>;
