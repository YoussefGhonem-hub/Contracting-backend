using Contracting.API.Controllers.Shared;
using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using Contracting.Shared.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contracting.API.Controllers;

[Route("api/backoffice/client-content")]
[ApiController]
[Authorize]
public class ClientContentManagementController : APIBaseController
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorage _fileStorage;

    public ClientContentManagementController(ApplicationDbContext db, IFileStorage fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    [HttpPost("monthly-reports")]
    [Authorize]
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
                    Url = "/" + relativePath.TrimStart('/')
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
                    Url = "/" + relativePath.TrimStart('/')
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

        if (!Enum.TryParse<DrawingType>(request.Type, true, out var type))
            return BadRequest(new { message = "Type must be TwoD or ThreeD." });

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
            Url = "/" + relativePath.TrimStart('/'),
            UploadedBy = userId
        };

        await _db.ProjectDrawings.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { entity.Id, entity.ProjectId, Type = entity.Type.ToString(), entity.Title, entity.Url });
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
            Url = "/" + relativePath.TrimStart('/'),
            UploadedBy = userId
        };

        await _db.TenderDocuments.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { entity.Id, entity.ProjectId, entity.Title, entity.Url });
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
            Url = "/" + relativePath.TrimStart('/'),
            UploadedBy = userId
        };

        await _db.ProjectSchedules.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { entity.Id, entity.ProjectId, entity.Title, entity.Version, entity.Url });
    }

    [HttpPost("invoices")]
    [Authorize(Roles = RoleNames.Accounts + "," + RoleNames.Admin + "," + RoleNames.SuperAdmin)]
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
    [Authorize(Roles = RoleNames.Accounts + "," + RoleNames.Admin + "," + RoleNames.SuperAdmin)]
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
}
