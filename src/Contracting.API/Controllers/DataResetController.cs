using Contracting.API.Controllers.Shared;
using Contracting.Infrustructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.API.Controllers;

/// <summary>
/// Wipes all transactional data while preserving master/configuration data.
/// Requires the X-Reset-Confirm: true header to prevent accidental calls.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DataResetController : APIBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storage;

    public DataResetController(ApplicationDbContext context, IStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    /// <summary>
    /// Removes all transactional data and their related AWS S3 files.
    ///
    /// TABLES CLEARED (transactional):
    ///   Business requests: EngineerRequests + Notes, Activities, Attachments, SpecialFieldValues, SpecialFieldItems, PurchaseRequestReceipts
    ///   Site reports:      EngineerSiteReports + WorkLogs, Attachments, Materials, Equipment, SurveyQuestions, ReportWorkers
    ///   Transfer:          TransferRequests + Items, Activities, Attachments
    ///   Financial:         FinancialClearances + Activities, Attachments
    ///   Labor:             LaborAttendanceRequests + Records, Activities, Attachments
    ///   Client portal:     ChatGroups/Members/Messages/Attachments,
    ///                      ClientMonthlyReports + Attachments, ProjectInvoices + Payments,
    ///                      VariationOrders + Attachments, TenderDocuments, ProjectSchedules,
    ///                      ProjectDrawings, ThreeDFolders + Images
    ///   Auth/helper:       RefreshTokens, UserDeviceTokens, PasswordResetCodes,
    ///                      NotificationLogs, ExceptionLogs, UserSignatures
    ///
    /// TABLES PRESERVED (master / configuration):
    ///   Branches, Departments, Engineers (ApplicationUsers), Projects,
    ///   EngineerProjects, EngineerDepartments, SpecialFields, DepartmentSpecialFields,
    ///   Priorities, Statuses, ConstructionItems, ConstructionItemUnits,
    ///   EngineerSiteSurveyQuestionTemplates, Clients, ClientProjects,
    ///   ApplicationRoles + identity tables
    /// </summary>
    [HttpDelete("reset-all-data")]
    [ProducesResponseType(typeof(DataResetResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetAllData(
        [FromHeader(Name = "X-Reset-Confirm")] string? confirm,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(confirm, "true", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Send header 'X-Reset-Confirm: true' to confirm this destructive operation.");

        var result = new DataResetResult();

        // ── Step 1: Collect all S3 keys before deleting DB records ──────────────

        var s3Keys = new List<string>();

        var requestAttachmentKeys = await _context.EngineerRequestAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var siteReportAttachmentKeys = await _context.EngineerSiteReportAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var workLogAttachmentKeys = await _context.EngineerSiteWorkLogAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var transferAttachmentKeys = await _context.TransferRequestAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var financialAttachmentKeys = await _context.FinancialClearanceAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var laborAttachmentKeys = await _context.LaborAttendanceAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var chatAttachmentKeys = await _context.ChatMessageAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var monthlyReportAttachmentKeys = await _context.ClientMonthlyReportAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var variationOrderAttachmentKeys = await _context.VariationOrderAttachments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var tenderDocumentKeys = await _context.TenderDocuments
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var projectScheduleKeys = await _context.ProjectSchedules
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var projectDrawingKeys = await _context.ProjectDrawings
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        var threeDImageKeys = await _context.ThreeDImages
            .IgnoreQueryFilters().Where(x => x.Key != null).Select(x => x.Key!).ToListAsync(cancellationToken);

        // UserSignature has no Key field — extract key from S3 URL path
        var signatureKeys = await _context.UserSignatures
            .IgnoreQueryFilters()
            .Where(x => x.SignatureUrl != null)
            .Select(x => x.SignatureUrl)
            .ToListAsync(cancellationToken);

        s3Keys.AddRange(requestAttachmentKeys);
        s3Keys.AddRange(siteReportAttachmentKeys);
        s3Keys.AddRange(workLogAttachmentKeys);
        s3Keys.AddRange(transferAttachmentKeys);
        s3Keys.AddRange(financialAttachmentKeys);
        s3Keys.AddRange(laborAttachmentKeys);
        s3Keys.AddRange(chatAttachmentKeys);
        s3Keys.AddRange(monthlyReportAttachmentKeys);
        s3Keys.AddRange(variationOrderAttachmentKeys);
        s3Keys.AddRange(tenderDocumentKeys);
        s3Keys.AddRange(projectScheduleKeys);
        s3Keys.AddRange(projectDrawingKeys);
        s3Keys.AddRange(threeDImageKeys);

        // Convert signature URLs → S3 keys by stripping the base URL prefix
        foreach (var url in signatureKeys)
        {
            if (string.IsNullOrWhiteSpace(url)) continue;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                s3Keys.Add(uri.AbsolutePath.TrimStart('/'));
        }

        result.S3FilesQueued = s3Keys.Count;

        // ── Step 2: Delete DB records (children first, then parents) ────────────
        // Using ExecuteDeleteAsync to bypass EF soft-delete interceptor and global query filters.

        // --- Engineer Requests ---
        result.EngineerRequestAttachments  = await _context.EngineerRequestAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerRequestSpecialFieldValues = await _context.EngineerRequestSpecialFieldValues.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerRequestSpecialFieldItems  = await _context.EngineerRequestSpecialFieldItems.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerRequestActivities   = await _context.EngineerRequestActivites.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.PurchaseRequestReceipts     = await _context.PurchaseRequestReceipts.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerRequestNotes        = await _context.EngineerRequestNotes.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerRequests            = await _context.EngineerRequests.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);

        // --- Site Reports ---
        result.ReportConstructionItemWorkers  = await _context.ReportConstructionItemWorkers.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerSiteWorkLogAttachments = await _context.EngineerSiteWorkLogAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerSiteEquipments         = await _context.EngineerSiteEquipments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerSiteMaterials          = await _context.EngineerSiteMaterials.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerSiteSurveyQuestions    = await _context.EngineerSiteSurveyQuestions.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerSiteWorkLogs           = await _context.EngineerSiteWorkLogs.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerSiteReportAttachments  = await _context.EngineerSiteReportAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.EngineerSiteReports            = await _context.EngineerSiteReports.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);

        // --- Transfer Requests ---
        result.TransferRequestAttachments = await _context.TransferRequestAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.TransferRequestActivities  = await _context.TransferRequestActivities.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.TransferRequestItems       = await _context.TransferRequestItems.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.TransferRequests           = await _context.TransferRequests.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);

        // --- Financial Clearance ---
        result.FinancialClearanceAttachments = await _context.FinancialClearanceAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.FinancialClearanceActivities  = await _context.FinancialClearanceActivities.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.FinancialClearances           = await _context.FinancialClearances.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);

        // --- Labor Attendance ---
        result.LaborAttendanceAttachments = await _context.LaborAttendanceAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.LaborAttendanceActivities  = await _context.LaborAttendanceActivities.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.LaborAttendanceRecords     = await _context.LaborAttendanceRecords.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.LaborAttendanceRequests    = await _context.LaborAttendanceRequests.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);

        // --- Client Portal ---
        result.ChatMessageAttachments         = await _context.ChatMessageAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ChatMessages                   = await _context.ChatMessages.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ChatGroupMembers               = await _context.ChatGroupMembers.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ChatGroups                     = await _context.ChatGroups.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ClientMonthlyReportAttachments = await _context.ClientMonthlyReportAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ClientMonthlyReports           = await _context.ClientMonthlyReports.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.InvoicePayments                = await _context.InvoicePayments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ProjectInvoices                = await _context.ProjectInvoices.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.VariationOrderAttachments      = await _context.VariationOrderAttachments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.VariationOrders                = await _context.VariationOrders.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.TenderDocuments                = await _context.TenderDocuments.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ProjectSchedules               = await _context.ProjectSchedules.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ProjectDrawings                = await _context.ProjectDrawings.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ThreeDImages                   = await _context.ThreeDImages.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ThreeDFolders                  = await _context.ThreeDFolders.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        // Clients and ClientProjects are preserved (treated as master data)

        // --- Auth / Helper ---
        result.RefreshTokens      = await _context.RefreshTokens.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.UserDeviceTokens   = await _context.userDeviceTokens.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.PasswordResetCodes = await _context.PasswordResetCodes.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.NotificationLogs   = await _context.NotificationLogs.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.ExceptionLogs      = await _context.ExceptionLogs.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        result.UserSignatures     = await _context.UserSignatures.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);

        // ── Step 3: Delete files from AWS S3 in parallel ────────────────────────

        var s3Errors = new List<string>();
        var deleteTasks = s3Keys
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .Select(async key =>
            {
                try { await _storage.Delete(key, cancellationToken); }
                catch (Exception ex) { lock (s3Errors) s3Errors.Add($"{key}: {ex.Message}"); }
            });

        await Task.WhenAll(deleteTasks);

        result.S3FilesDeleted = result.S3FilesQueued - s3Errors.Count;
        result.S3Errors = s3Errors;

        return Ok(result);
    }
}

public sealed class DataResetResult
{
    // S3
    public int S3FilesQueued  { get; set; }
    public int S3FilesDeleted { get; set; }
    public List<string> S3Errors { get; set; } = new();

    // Engineer Requests
    public int EngineerRequests             { get; set; }
    public int EngineerRequestNotes         { get; set; }
    public int EngineerRequestActivities    { get; set; }
    public int EngineerRequestAttachments   { get; set; }
    public int EngineerRequestSpecialFieldValues { get; set; }
    public int EngineerRequestSpecialFieldItems  { get; set; }
    public int PurchaseRequestReceipts      { get; set; }

    // Site Reports
    public int EngineerSiteReports            { get; set; }
    public int EngineerSiteReportAttachments  { get; set; }
    public int EngineerSiteWorkLogs           { get; set; }
    public int EngineerSiteWorkLogAttachments { get; set; }
    public int EngineerSiteMaterials          { get; set; }
    public int EngineerSiteEquipments         { get; set; }
    public int EngineerSiteSurveyQuestions    { get; set; }
    public int ReportConstructionItemWorkers  { get; set; }

    // Transfer Requests
    public int TransferRequests           { get; set; }
    public int TransferRequestItems       { get; set; }
    public int TransferRequestActivities  { get; set; }
    public int TransferRequestAttachments { get; set; }

    // Financial Clearance
    public int FinancialClearances           { get; set; }
    public int FinancialClearanceActivities  { get; set; }
    public int FinancialClearanceAttachments { get; set; }

    // Labor Attendance
    public int LaborAttendanceRequests    { get; set; }
    public int LaborAttendanceRecords     { get; set; }
    public int LaborAttendanceActivities  { get; set; }
    public int LaborAttendanceAttachments { get; set; }

    // Client Portal (Clients + ClientProjects are preserved)
    public int ChatGroups                     { get; set; }
    public int ChatGroupMembers               { get; set; }
    public int ChatMessages                   { get; set; }
    public int ChatMessageAttachments         { get; set; }
    public int ClientMonthlyReports           { get; set; }
    public int ClientMonthlyReportAttachments { get; set; }
    public int ProjectInvoices                { get; set; }
    public int InvoicePayments                { get; set; }
    public int VariationOrders                { get; set; }
    public int VariationOrderAttachments      { get; set; }
    public int TenderDocuments                { get; set; }
    public int ProjectSchedules               { get; set; }
    public int ProjectDrawings                { get; set; }
    public int ThreeDFolders                  { get; set; }
    public int ThreeDImages                   { get; set; }

    // Auth / Helper
    public int RefreshTokens      { get; set; }
    public int UserDeviceTokens   { get; set; }
    public int PasswordResetCodes { get; set; }
    public int NotificationLogs   { get; set; }
    public int ExceptionLogs      { get; set; }
    public int UserSignatures     { get; set; }
}
