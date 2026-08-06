using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Application.Features.Business.Common;

/// <summary>
/// Shared plumbing for the anonymous printable-report handlers: terminal-status
/// detection, signatory construction, and signature-URL resolution
/// (Engineer → ApplicationUser → UserSignature → pre-signed S3 URL).
/// </summary>
public static class PublicReportHelpers
{
    public static readonly string[] CompletedKeywords = { "completed", "complete", "done", "finished", "finish", "closed" };
    public static readonly string[] RejectedKeywords = { "rejected", "reject", "denied", "deny", "cancelled", "cancel" };

    public static bool StatusMatches(GetDropDownStatusDto? status, string[] keywords)
        => status != null && keywords.Any(k =>
            (status.Code?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameEn?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameAr?.Contains(k, StringComparison.OrdinalIgnoreCase) == true));

    public static RequestSignatoryDto? BuildSignatory(GetEngineerDto? engineer, DateTimeOffset? signedDate, string decision)
        => engineer is null ? null : new RequestSignatoryDto
        {
            EngineerId = engineer.Id,
            NameEn = engineer.nameEn,
            NameAr = engineer.nameAr,
            Position = engineer.position,
            SignedDate = signedDate,
            Decision = decision
        };

    public static async Task AttachSignatureUrlsAsync(
        ApplicationDbContext db,
        IStorageService s3,
        IEnumerable<RequestSignatoryDto?> signatories,
        CancellationToken cancellationToken)
    {
        var withEngineer = signatories
            .Where(s => s?.EngineerId != null)
            .Cast<RequestSignatoryDto>()
            .ToList();

        if (withEngineer.Count == 0) return;

        var engineerIds = withEngineer.Select(s => s.EngineerId!.Value).Distinct().ToList();

        var signatures = await (
                from e in db.Engineers
                join s in db.UserSignatures on e.ApplicationUserId equals s.UserId
                where engineerIds.Contains(e.Id)
                select new { EngineerId = e.Id, s.SignatureUrl })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var signatory in withEngineer)
        {
            var key = signatures.FirstOrDefault(x => x.EngineerId == signatory.EngineerId)?.SignatureUrl;
            if (!string.IsNullOrWhiteSpace(key))
                signatory.SignatureUrl = s3.GetPreSignedUrl(key);
        }
    }
}
