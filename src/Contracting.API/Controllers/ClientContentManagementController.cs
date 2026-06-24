using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.ClientContent.Command.UpdateMonthlyReport;
using Contracting.Application.Features.Business.ClientContent.Command.UpdateSchedule;
using Contracting.Application.Features.Business.ClientContent.Command.UpdateTenderDocument;
using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using Contracting.Shared.Storage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storage.AWS3.Services;
using Microsoft.EntityFrameworkCore;

namespace Contracting.API.Controllers;

[Route("api/backoffice/client-content")]
[ApiController]
[Authorize]
public class ClientContentManagementController : APIBaseController
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorage _fileStorage;
    private readonly IStorageService _storage;
    private readonly IMediator _mediator;

    public ClientContentManagementController(ApplicationDbContext db, IFileStorage fileStorage, IStorageService storage, IMediator mediator)
    {
        _db = db;
        _fileStorage = fileStorage;
        _storage = storage;
        _mediator = mediator;
    }

    [HttpPost("monthly-reports")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMonthlyReport([FromForm] CreateMonthlyReportRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            return NotFound(new { message = "Project not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var report = new ClientMonthlyReport
        {
            ProjectId = request.ProjectId,
            Month = request.Month,
            Year = request.Year,
            Title = request.Title,
            WorkProgress = request.WorkProgress,
            UploadedBy = userId,
            Attachments = new List<ClientMonthlyReportAttachment>()
        };

        if (request.Attachments is not null)
        {
            foreach (var file in request.Attachments.Where(f => f is not null && f.Length > 0))
            {
                var relativePath = await _fileStorage.SaveAsync(file, "uploads/monthly-reports", cancellationToken);
                report.Attachments.Add(new ClientMonthlyReportAttachment
                {
                    Key = relativePath,
                    FileName = file.FileName,
                    Extension = Path.GetExtension(file.FileName),
                    FileSize = file.Length,
                    Url = _storage.GetUploadedFileUrl(relativePath)
                });
            }
        }

        await _db.ClientMonthlyReports.AddAsync(report, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { report.Id, report.ProjectId, report.Month, report.Year, report.Title, report.WorkProgress });
    }

    [HttpPost("variation-orders")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateVariationOrder([FromForm] CreateBackofficeVariationOrderRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            return NotFound(new { message = "Project not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var engineer = await _db.Engineers.FirstOrDefaultAsync(e => e.ApplicationUserId == userId, cancellationToken);
        if (engineer is null)
            return BadRequest(new { message = "Current user is not a technical office engineer." });

        var nextNumber = (await _db.VariationOrders
            .Where(v => v.ProjectId == request.ProjectId)
            .MaxAsync(v => (int?)v.VONumber, cancellationToken) ?? 0) + 1;

        var entity = new VariationOrder
        {
            ProjectId = request.ProjectId,
            VONumber = nextNumber,
            Title = request.Title,
            Description = request.Description,
            Cost = request.Cost,
            IssueDate = request.IssueDate ?? DateTimeHelper.DateTimeNow,
            DueDate = request.DueDate,
            Status = VOStatus.Pending,
            CreatedByEngineerId = engineer.Id,
            Attachments = new List<VariationOrderAttachment>()
        };

        if (request.Attachments is not null)
        {
            foreach (var file in request.Attachments.Where(f => f is not null && f.Length > 0))
            {
                var relativePath = await _fileStorage.SaveAsync(file, "uploads/variation-orders", cancellationToken);
                entity.Attachments.Add(new VariationOrderAttachment
                {
                    Key = relativePath,
                    FileName = file.FileName,
                    Extension = Path.GetExtension(file.FileName),
                    FileSize = file.Length,
                    Url = _storage.GetUploadedFileUrl(relativePath)
                });
            }
        }

        await _db.VariationOrders.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { entity.Id, entity.ProjectId, entity.VONumber, entity.Title, entity.Status });
    }

    [HttpPost("drawings")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDrawing([FromForm] UploadDrawingRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            return NotFound(new { message = "Project not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        if (!Enum.TryParse<DrawingType>(request.Type, true, out var type) || type == DrawingType.ThreeD)
            return BadRequest(new { message = "Type must be TwoD. Use the 3D folder API for 3D drawings." });

        var relativePath = await _fileStorage.SaveAsync(request.File, "uploads/drawings", cancellationToken);

        var entity = new ProjectDrawing
        {
            ProjectId = request.ProjectId,
            Type = type,
            Title = request.Title,
            Key = relativePath,
            FileName = request.File.FileName,
            Extension = Path.GetExtension(request.File.FileName),
            FileSize = request.File.Length,
            Url = _storage.GetUploadedFileUrl(relativePath),
            UploadedBy = userId
        };

        await _db.ProjectDrawings.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { entity.Id, entity.ProjectId, Type = entity.Type.ToString(), entity.Title, Url = _storage.GetPreSignedUrl(entity.Key) ?? entity.Url });
    }

    [HttpPost("tender-documents")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadTender([FromForm] UploadTenderDocumentRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            return NotFound(new { message = "Project not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var relativePath = await _fileStorage.SaveAsync(request.File, "uploads/tender", cancellationToken);

        var entity = new TenderDocument
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Key = relativePath,
            FileName = request.File.FileName,
            Extension = Path.GetExtension(request.File.FileName),
            FileSize = request.File.Length,
            Url = _storage.GetUploadedFileUrl(relativePath),
            UploadedBy = userId
        };

        await _db.TenderDocuments.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { entity.Id, entity.ProjectId, entity.Title, Url = _storage.GetPreSignedUrl(entity.Key) ?? entity.Url });
    }

    [HttpPost("schedules")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadSchedule([FromForm] UploadScheduleRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            return NotFound(new { message = "Project not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var relativePath = await _fileStorage.SaveAsync(request.File, "uploads/schedules", cancellationToken);

        var entity = new ProjectSchedule
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Version = request.Version,
            Key = relativePath,
            FileName = request.File.FileName,
            Extension = Path.GetExtension(request.File.FileName),
            FileSize = request.File.Length,
            Url = _storage.GetUploadedFileUrl(relativePath),
            UploadedBy = userId
        };

        await _db.ProjectSchedules.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { entity.Id, entity.ProjectId, entity.Title, entity.Version, Url = _storage.GetPreSignedUrl(entity.Key) ?? entity.Url });
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            return NotFound(new { message = "Project not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var nextNumber = (await _db.ProjectInvoices
            .Where(i => i.ProjectId == request.ProjectId)
            .MaxAsync(i => (int?)i.InvoiceNumber, cancellationToken) ?? 0) + 1;

        var status = request.PaidAmount <= 0
            ? PaymentStatus.Pending
            : request.PaidAmount >= request.TotalValue
                ? PaymentStatus.Paid
                : PaymentStatus.PartiallyPaid;

        var invoice = new ProjectInvoice
        {
            ProjectId = request.ProjectId,
            InvoiceNumber = nextNumber,
            Title = request.Title,
            TotalValue = request.TotalValue,
            PaidAmount = request.PaidAmount,
            Status = status,
            Notes = request.Notes,
            IssueDate = request.IssueDate ?? DateTimeHelper.DateTimeNow,
            DueDate = request.DueDate,
            UpdatedBy = userId
        };

        await _db.ProjectInvoices.AddAsync(invoice, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            invoice.Id,
            invoice.ProjectId,
            invoice.InvoiceNumber,
            invoice.Title,
            invoice.TotalValue,
            invoice.PaidAmount,
            Status = invoice.Status.ToString(),
            invoice.IssueDate,
            invoice.DueDate
        });
    }

    [HttpPut("invoices/{invoiceId:guid}/payment")]
    public async Task<IActionResult> UpdateInvoicePayment(Guid invoiceId, [FromBody] UpdateInvoicePaymentRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _db.ProjectInvoices.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        if (invoice is null)
            return NotFound(new { message = "Invoice not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        invoice.PaidAmount = request.PaidAmount;
        invoice.Status = request.PaidAmount <= 0
            ? PaymentStatus.Pending
            : request.PaidAmount >= invoice.TotalValue
                ? PaymentStatus.Paid
                : PaymentStatus.PartiallyPaid;
        invoice.Notes = request.Notes;
        invoice.UpdatedBy = userId;
        invoice.ModifiedDate = DateTimeHelper.DateTimeNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            invoice.Id,
            invoice.ProjectId,
            invoice.InvoiceNumber,
            invoice.TotalValue,
            invoice.PaidAmount,
            Status = invoice.Status.ToString(),
            invoice.Notes
        });
    }

    // =========================================================================
    // PUT /api/backoffice/client-content/invoices/{invoiceId}
    // Full edit of an invoice. Status auto-recalculates from amounts.
    // =========================================================================
    [HttpPut("invoices/{invoiceId:guid}")]
    public async Task<IActionResult> UpdateInvoice(Guid invoiceId, [FromBody] UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _db.ProjectInvoices.FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, cancellationToken);
        if (invoice is null)
            return NotFound(new { message = "Invoice not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        invoice.Title = request.Title;
        invoice.TotalValue = request.TotalValue;
        invoice.PaidAmount = request.PaidAmount;
        invoice.Notes = request.Notes;
        invoice.IssueDate = request.IssueDate ?? invoice.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.Status = request.PaidAmount <= 0
            ? PaymentStatus.Pending
            : request.PaidAmount >= request.TotalValue
                ? PaymentStatus.Paid
                : PaymentStatus.PartiallyPaid;
        invoice.UpdatedBy = userId;
        invoice.MarkAsModified(userId);

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            invoice.Id,
            invoice.ProjectId,
            invoice.InvoiceNumber,
            invoice.Title,
            invoice.TotalValue,
            invoice.PaidAmount,
            Status = invoice.Status.ToString(),
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Notes
        });
    }

    // =========================================================================
    // DELETE /api/backoffice/client-content/invoices/{invoiceId}
    // =========================================================================
    [HttpDelete("invoices/{invoiceId:guid}")]
    public async Task<IActionResult> DeleteInvoice(Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _db.ProjectInvoices.FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, cancellationToken);
        if (invoice is null)
            return NotFound(new { message = "Invoice not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        invoice.MarkAsDeleted(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Invoice deleted successfully." });
    }

    // =========================================================================
    // PUT /api/backoffice/client-content/variation-orders/{voId}
    // Edit allowed ONLY while the VO is still Pending.
    // =========================================================================
    [HttpPut("variation-orders/{voId:guid}")]
    public async Task<IActionResult> UpdateVariationOrder(Guid voId, [FromBody] UpdateVariationOrderRequest request, CancellationToken cancellationToken)
    {
        var vo = await _db.VariationOrders.FirstOrDefaultAsync(v => v.Id == voId && !v.IsDeleted, cancellationToken);
        if (vo is null)
            return NotFound(new { message = "Variation order not found." });

        if (vo.Status != VOStatus.Pending)
            return Conflict(new { message = "Cannot modify — already actioned." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        vo.Title = request.Title;
        vo.Description = request.Description;
        vo.Cost = request.Cost;
        vo.IssueDate = request.IssueDate ?? vo.IssueDate;
        vo.DueDate = request.DueDate;
        vo.MarkAsModified(userId);

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            vo.Id,
            vo.ProjectId,
            vo.VONumber,
            vo.Title,
            vo.Description,
            vo.Cost,
            Status = vo.Status.ToString(),
            vo.IssueDate,
            vo.DueDate
        });
    }

    // =========================================================================
    // DELETE /api/backoffice/client-content/variation-orders/{voId}
    // Delete allowed ONLY while the VO is still Pending.
    // =========================================================================
    [HttpDelete("variation-orders/{voId:guid}")]
    public async Task<IActionResult> DeleteVariationOrder(Guid voId, CancellationToken cancellationToken)
    {
        var vo = await _db.VariationOrders.FirstOrDefaultAsync(v => v.Id == voId && !v.IsDeleted, cancellationToken);
        if (vo is null)
            return NotFound(new { message = "Variation order not found." });

        if (vo.Status != VOStatus.Pending)
            return Conflict(new { message = "Cannot modify — already actioned." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        vo.MarkAsDeleted(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Variation order deleted successfully." });
    }

    // =========================================================================
    // PUT /api/backoffice/client-content/drawings/{drawingId}
    // Update title and optionally replace the stored file.
    // =========================================================================
    [HttpPut("drawings/{drawingId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateDrawing(Guid drawingId, [FromForm] UpdateDrawingRequest request, CancellationToken cancellationToken)
    {
        var drawing = await _db.ProjectDrawings.FirstOrDefaultAsync(d => d.Id == drawingId && !d.IsDeleted, cancellationToken);
        if (drawing is null)
            return NotFound(new { message = "Drawing not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        drawing.Title = request.Title;

        if (request.File is not null && request.File.Length > 0)
        {
            // Delete old file from S3
            if (!string.IsNullOrWhiteSpace(drawing.Key))
                await _fileStorage.DeleteAsync(drawing.Key, cancellationToken);

            var relativePath = await _fileStorage.SaveAsync(request.File, "uploads/drawings", cancellationToken);
            drawing.Key = relativePath;
            drawing.FileName = request.File.FileName;
            drawing.Extension = Path.GetExtension(request.File.FileName);
            drawing.FileSize = request.File.Length;
            drawing.Url = _storage.GetUploadedFileUrl(relativePath);
        }

        drawing.MarkAsModified(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            drawing.Id,
            drawing.ProjectId,
            Type = drawing.Type.ToString(),
            drawing.Title,
            drawing.FileName,
            drawing.FileSize,
            drawing.Url
        });
    }

    // =========================================================================
    // DELETE /api/backoffice/client-content/drawings/{drawingId}
    // =========================================================================
    [HttpDelete("drawings/{drawingId:guid}")]
    public async Task<IActionResult> DeleteDrawing(Guid drawingId, CancellationToken cancellationToken)
    {
        var drawing = await _db.ProjectDrawings.FirstOrDefaultAsync(d => d.Id == drawingId && !d.IsDeleted, cancellationToken);
        if (drawing is null)
            return NotFound(new { message = "Drawing not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        drawing.MarkAsDeleted(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Drawing deleted successfully." });
    }

    // =========================================================================
    // PUT /api/backoffice/client-content/schedules/{scheduleId}
    // Update title/version and optionally replace the stored file.
    // =========================================================================
    [HttpPut("schedules/{scheduleId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateSchedule(Guid scheduleId, [FromForm] UpdateScheduleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateScheduleCommand(scheduleId, request.Title, request.Version, request.File);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match(value => Ok(value), errors => Problem(errors));
    }

    // =========================================================================
    // DELETE /api/backoffice/client-content/schedules/{scheduleId}
    // =========================================================================
    [HttpDelete("schedules/{scheduleId:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid scheduleId, CancellationToken cancellationToken)
    {
        var schedule = await _db.ProjectSchedules.FirstOrDefaultAsync(s => s.Id == scheduleId && !s.IsDeleted, cancellationToken);
        if (schedule is null)
            return NotFound(new { message = "Schedule not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        schedule.MarkAsDeleted(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Schedule deleted successfully." });
    }

    // =========================================================================
    // PUT /api/backoffice/client-content/tender-documents/{tenderId}
    // Update title and optionally replace the stored file.
    // =========================================================================
    [HttpPut("tender-documents/{tenderId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateTenderDocument(Guid tenderId, [FromForm] UpdateTenderDocumentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTenderDocumentCommand(tenderId, request.Title, request.File);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match(value => Ok(value), errors => Problem(errors));
    }

    // =========================================================================
    // DELETE /api/backoffice/client-content/tender-documents/{tenderId}
    // =========================================================================
    [HttpDelete("tender-documents/{tenderId:guid}")]
    public async Task<IActionResult> DeleteTenderDocument(Guid tenderId, CancellationToken cancellationToken)
    {
        var tender = await _db.TenderDocuments.FirstOrDefaultAsync(t => t.Id == tenderId && !t.IsDeleted, cancellationToken);
        if (tender is null)
            return NotFound(new { message = "Tender document not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        tender.MarkAsDeleted(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Tender document deleted successfully." });
    }

    // =========================================================================
    // PUT /api/backoffice/client-content/monthly-reports/{reportId}
    // Update metadata, append new attachments, and/or remove existing attachments.
    // =========================================================================
    [HttpPut("monthly-reports/{reportId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMonthlyReport(Guid reportId, [FromForm] UpdateMonthlyReportRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMonthlyReportCommand(
            reportId, request.Month, request.Year, request.Title, request.WorkProgress,
            request.Attachments, request.RemoveAttachmentIds);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match(value => Ok(value), errors => Problem(errors));
    }

    // =========================================================================
    // DELETE /api/backoffice/client-content/monthly-reports/{reportId}
    // =========================================================================
    [HttpDelete("monthly-reports/{reportId:guid}")]
    public async Task<IActionResult> DeleteMonthlyReport(Guid reportId, CancellationToken cancellationToken)
    {
        var report = await _db.ClientMonthlyReports.FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);
        if (report is null)
            return NotFound(new { message = "Report not found." });

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        report.MarkAsDeleted(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Monthly report deleted successfully." });
    }
}
