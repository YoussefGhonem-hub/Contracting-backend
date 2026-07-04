using Contracting.API.Controllers.Shared;
using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common.Enums;
using Contracting.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.API.Controllers;

/// <summary>
/// Back-office read endpoints for the Project Configuration page.
/// Provides GET lists for every section: invoices, drawings, schedules,
/// tender documents, variation orders, monthly reports and chat group.
/// </summary>
[Route("api/backoffice/projects/{projectId:guid}")]
[ApiController]
[Authorize]
public class BackofficeProjectConfigController : APIBaseController
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public BackofficeProjectConfigController(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/configuration
    // Overview snapshot: project info + item counts per section
    // =========================================================================
    [HttpGet("configuration")]
    public async Task<IActionResult> GetConfiguration(Guid projectId, CancellationToken ct)
    {
        var project = await _db.Projects
            .Where(p => p.Id == projectId && !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                NameEn        = p.nameEn,
                NameAr        = p.nameAr,
                Location      = p.location,
                p.Code,
                ImageUrl      = p.imageUrl,
                p.Area,
                p.ContractValue,
                p.ProgressPercent,
                p.StartDate,
                p.ExpectedEndDate,
                ProjectStatus = p.ProjectStatus != null ? p.ProjectStatus.ToString() : null
            })
            .FirstOrDefaultAsync(ct);

        if (project is null)
            return NotFound(new { message = "Project not found." });

        var chatGroup = await _db.ChatGroups
            .Where(cg => cg.ProjectId == projectId && !cg.IsDeleted)
            .Select(cg => new { cg.Id })
            .FirstOrDefaultAsync(ct);

        var chatMemberCount  = chatGroup is not null
            ? await _db.ChatGroupMembers.CountAsync(m => m.ChatGroupId == chatGroup.Id && !m.IsDeleted, ct)
            : 0;

        var invoiceCount     = await _db.ProjectInvoices.CountAsync(i => i.ProjectId == projectId && !i.IsDeleted, ct);
        var twoDCount        = await _db.ProjectDrawings.CountAsync(d => d.ProjectId == projectId && d.Type == DrawingType.TwoD    && !d.IsDeleted, ct);
        var threeDCount      = await _db.ProjectDrawings.CountAsync(d => d.ProjectId == projectId && d.Type == DrawingType.ThreeD  && !d.IsDeleted, ct);
        var scheduleCount    = await _db.ProjectSchedules.CountAsync(s => s.ProjectId == projectId && !s.IsDeleted, ct);
        var tenderCount      = await _db.TenderDocuments.CountAsync(t => t.ProjectId == projectId && !t.IsDeleted, ct);
        var voCount          = await _db.VariationOrders.CountAsync(v => v.ProjectId == projectId && !v.IsDeleted, ct);
        var pendingVOCount   = await _db.VariationOrders.CountAsync(v => v.ProjectId == projectId && v.Status == VOStatus.Pending && !v.IsDeleted, ct);
        var reportCount      = await _db.ClientMonthlyReports.CountAsync(r => r.ProjectId == projectId && !r.IsDeleted, ct);

        return Ok(new
        {
            project.Id,
            project.NameEn,
            project.NameAr,
            project.Location,
            project.Code,
            project.ImageUrl,
            project.Area,
            project.ContractValue,
            project.ProgressPercent,
            project.StartDate,
            project.ExpectedEndDate,
            project.ProjectStatus,
            Sections = new
            {
                ChatGroupExists     = chatGroup is not null,
                ChatMemberCount     = chatMemberCount,
                InvoiceCount        = invoiceCount,
                TwoDDrawingCount    = twoDCount,
                ThreeDDrawingCount  = threeDCount,
                ScheduleCount       = scheduleCount,
                TenderDocumentCount = tenderCount,
                VariationOrderCount = voCount,
                PendingVOCount      = pendingVOCount,
                MonthlyReportCount  = reportCount
            }
        });
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/invoices
    // =========================================================================
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices(Guid projectId, [FromQuery] string? status = null, CancellationToken ct = default)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct))
            return NotFound(new { message = "Project not found." });

        var query = _db.ProjectInvoices
            .Include(i => i.Payments)
            .Include(i => i.Attachments)
            .Where(i => i.ProjectId == projectId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var ps))
            query = query.Where(i => i.Status == ps);

        var invoices = await query
            .OrderBy(i => i.InvoiceNumber)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.Title,
                i.TotalValue,
                i.PaidAmount,
                Status   = i.Status.ToString(),
                i.IssueDate,
                i.DueDate,
                i.Notes,
                Payments = i.Payments
                    .Where(p => !p.IsDeleted)
                    .Select(p => new { p.Id, p.Amount, p.PaymentDate, p.Reference, p.Notes })
                    .ToList(),
                Attachments = i.Attachments
                    .Select(a => new { a.Id, a.FileName, a.Url })
                    .ToList()
            })
            .ToListAsync(ct);

        var proj = await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new { p.ContractValue })
            .FirstOrDefaultAsync(ct);

        var approvedVOs = await _db.VariationOrders
            .Where(v => v.ProjectId == projectId && v.Status == VOStatus.Approved && !v.IsDeleted)
            .SumAsync(v => (decimal?)v.Cost, ct) ?? 0;

        var allInvoices = await _db.ProjectInvoices
            .Where(i => i.ProjectId == projectId && !i.IsDeleted)
            .Select(i => new { i.TotalValue, i.PaidAmount })
            .ToListAsync(ct);

        var totalInvoiced     = allInvoices.Sum(i => i.TotalValue);
        var totalPaid         = allInvoices.Sum(i => i.PaidAmount);
        var initialContract   = proj?.ContractValue ?? 0;
        var totalContractValue = initialContract + approvedVOs;
        var settledPercent    = totalContractValue > 0
            ? Math.Round((double)totalPaid / (double)totalContractValue * 100, 2)
            : 0;

        return Ok(new
        {
            FinancialSummary = new
            {
                InitialContractValue = initialContract,
                ApprovedVariations   = approvedVOs,
                TotalContractValue   = totalContractValue,
                TotalValue           = totalContractValue,
                TotalInvoiced        = totalInvoiced,
                TotalPaid            = totalPaid,
                RemainingAmount      = totalContractValue - totalPaid,
                SettledPercent       = settledPercent
            },
            Invoices = invoices
        });
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/drawings?type=TwoD|ThreeD
    // =========================================================================
    [HttpGet("drawings")]
    public async Task<IActionResult> GetDrawings(Guid projectId, [FromQuery] string? type = null, CancellationToken ct = default)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct))
            return NotFound(new { message = "Project not found." });

        var query = _db.ProjectDrawings.Where(d => d.ProjectId == projectId && !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<DrawingType>(type, true, out var dt))
            query = query.Where(d => d.Type == dt);

        var drawings = (await query
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new { d.Id, d.Title, Type = d.Type.ToString(), d.FileName, d.Extension, d.FileSize, d.Key, d.Url, UploadedAt = d.CreatedDate })
            .ToListAsync(ct))
            .Select(d => new
            {
                d.Id,
                d.Title,
                d.Type,
                d.FileName,
                d.Extension,
                d.FileSize,
                Url        = _storage.GetPreSignedUrl(d.Key) ?? d.Url,
                d.UploadedAt
            });

        return Ok(drawings);
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/schedules
    // =========================================================================
    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules(Guid projectId, CancellationToken ct)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct))
            return NotFound(new { message = "Project not found." });

        var schedules = (await _db.ProjectSchedules
            .Where(s => s.ProjectId == projectId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedDate)
            .Select(s => new { s.Id, s.Title, s.Version, s.FileName, s.Extension, s.FileSize, s.Key, s.Url, UploadedAt = s.CreatedDate })
            .ToListAsync(ct))
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Version,
                s.FileName,
                s.Extension,
                s.FileSize,
                Url        = _storage.GetPreSignedUrl(s.Key) ?? s.Url,
                s.UploadedAt
            });

        return Ok(schedules);
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/tender-documents
    // =========================================================================
    [HttpGet("tender-documents")]
    public async Task<IActionResult> GetTenderDocuments(Guid projectId, CancellationToken ct)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct))
            return NotFound(new { message = "Project not found." });

        var tenders = (await _db.TenderDocuments
            .Where(t => t.ProjectId == projectId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedDate)
            .Select(t => new { t.Id, t.Title, t.FileName, t.Extension, t.FileSize, t.Key, t.Url, UploadedAt = t.CreatedDate })
            .ToListAsync(ct))
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.FileName,
                t.Extension,
                t.FileSize,
                Url        = _storage.GetPreSignedUrl(t.Key) ?? t.Url,
                t.UploadedAt
            });

        return Ok(tenders);
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/variation-orders?status=Pending|Approved|Rejected
    // =========================================================================
    [HttpGet("variation-orders")]
    public async Task<IActionResult> GetVariationOrders(Guid projectId, [FromQuery] string? status = null, CancellationToken ct = default)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct))
            return NotFound(new { message = "Project not found." });

        var query = _db.VariationOrders
            .Include(v => v.Attachments)
            .Where(v => v.ProjectId == projectId && !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VOStatus>(status, true, out var vs))
            query = query.Where(v => v.Status == vs);

        var vosRaw = await query
            .OrderByDescending(v => v.VONumber)
            .Select(v => new
            {
                v.Id,
                v.VONumber,
                v.Title,
                v.Description,
                v.Cost,
                Status               = v.Status.ToString(),
                v.IssueDate,
                v.DueDate,
                v.ClientActionDate,
                v.ClientRejectionReason,
                Attachments = v.Attachments
                    .Where(a => !a.IsDeleted)
                    .Select(a => new { a.Id, a.FileName, a.Extension, a.FileSize, a.Key, a.Url })
                    .ToList()
            })
            .ToListAsync(ct);

        var vos = vosRaw.Select(v => new
        {
            v.Id,
            v.VONumber,
            v.Title,
            v.Description,
            v.Cost,
            v.Status,
            v.IssueDate,
            v.DueDate,
            v.ClientActionDate,
            v.ClientRejectionReason,
            Attachments = v.Attachments.Select(a => new
            {
                a.Id,
                a.FileName,
                a.Extension,
                a.FileSize,
                Url = _storage.GetPreSignedUrl(a.Key) ?? a.Url
            }).ToList()
        }).ToList();

        var allVOs = await _db.VariationOrders
            .Where(v => v.ProjectId == projectId && !v.IsDeleted)
            .Select(v => new { v.Status, v.Cost })
            .ToListAsync(ct);

        return Ok(new
        {
            Summary = new
            {
                TotalApproved = allVOs.Where(v => v.Status == VOStatus.Approved).Sum(v => v.Cost),
                TotalPending  = allVOs.Where(v => v.Status == VOStatus.Pending).Sum(v => v.Cost),
                TotalRejected = allVOs.Where(v => v.Status == VOStatus.Rejected).Sum(v => v.Cost)
            },
            VariationOrders = vos
        });
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/monthly-reports
    // =========================================================================
    [HttpGet("monthly-reports")]
    public async Task<IActionResult> GetMonthlyReports(Guid projectId, CancellationToken ct)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct))
            return NotFound(new { message = "Project not found." });

        var reports = await _db.ClientMonthlyReports
            .Where(r => r.ProjectId == projectId && !r.IsDeleted)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Select(r => new
            {
                r.Id,
                r.Title,
                r.Month,
                r.Year,
                r.CreatedDate,
                AttachmentCount = r.Attachments.Count(a => !a.IsDeleted)
            })
            .ToListAsync(ct);

        return Ok(reports);
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/monthly-reports/{reportId}
    // =========================================================================
    [HttpGet("monthly-reports/{reportId:guid}")]
    public async Task<IActionResult> GetMonthlyReportById(Guid projectId, Guid reportId, CancellationToken ct)
    {
        var reportRaw = await _db.ClientMonthlyReports
            .Where(r => r.Id == reportId && r.ProjectId == projectId && !r.IsDeleted)
            .Select(r => new
            {
                r.Id,
                r.ProjectId,
                r.Title,
                r.WorkProgress,
                r.Month,
                r.Year,
                r.CreatedDate,
                Attachments = r.Attachments
                    .Where(a => !a.IsDeleted)
                    .Select(a => new { a.Id, a.FileName, a.Extension, a.FileSize, a.Key, a.Url })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (reportRaw is null)
            return NotFound(new { message = "Report not found." });

        var report = new
        {
            reportRaw.Id,
            reportRaw.ProjectId,
            reportRaw.Title,
            reportRaw.WorkProgress,
            reportRaw.Month,
            reportRaw.Year,
            reportRaw.CreatedDate,
            Attachments = reportRaw.Attachments.Select(a => new
            {
                a.Id,
                a.FileName,
                a.Extension,
                a.FileSize,
                Url = _storage.GetPreSignedUrl(a.Key) ?? a.Url
            }).ToList()
        };

        return Ok(report);
    }

    // =========================================================================
    // GET /api/backoffice/projects/{projectId}/chat
    // Returns chat group info + full member list (userId, name, email, memberType)
    // =========================================================================
    [HttpGet("chat")]
    public async Task<IActionResult> GetChatGroup(Guid projectId, CancellationToken ct)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct))
            return NotFound(new { message = "Project not found." });

        var chatGroup = await _db.ChatGroups
            .Where(cg => cg.ProjectId == projectId && !cg.IsDeleted)
            .Select(cg => new { cg.Id, cg.ProjectId, cg.Name })
            .FirstOrDefaultAsync(ct);

        if (chatGroup is null)
            return Ok(new { Exists = false, message = "No chat group created yet for this project." });

        var members = await (
            from m in _db.ChatGroupMembers
            join u in _db.Users on m.ApplicationUserId equals u.Id
            where m.ChatGroupId == chatGroup.Id && !m.IsDeleted
            select new
            {
                UserId     = u.Id,
                u.FullName,
                u.Email,
                u.AvatarUrl,
                m.MemberType
            }
        ).ToListAsync(ct);

        return Ok(new
        {
            Exists = true,
            Group  = new { chatGroup.Id, chatGroup.ProjectId, chatGroup.Name, Members = members }
        });
    }
}
