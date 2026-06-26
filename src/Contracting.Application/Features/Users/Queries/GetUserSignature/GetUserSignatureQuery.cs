using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Application.Features.Users.Queries.GetUserSignature;

public record GetUserSignatureQuery : IRequest<ErrorOr<UserSignatureDto>>;

public class GetUserSignatureQueryHandler : IRequestHandler<GetUserSignatureQuery, ErrorOr<UserSignatureDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _s3;

    public GetUserSignatureQueryHandler(ApplicationDbContext db, IStorageService s3)
    {
        _db = db;
        _s3 = s3;
    }

    public async Task<ErrorOr<UserSignatureDto>> Handle(GetUserSignatureQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        if (!currentUserId.HasValue)
            return Error.Unauthorized("Signature.Unauthorized", "User is not authenticated.");

        var signature = await _db.UserSignatures
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId.Value, cancellationToken);

        if (signature is null)
            return Error.NotFound("Signature.NotFound", "Signature not found.");

        return new UserSignatureDto
        {
            Id = signature.Id,
            UserId = signature.UserId,
            SignatureUrl = string.IsNullOrWhiteSpace(signature.SignatureUrl)
                ? signature.SignatureUrl
                : _s3.GetPreSignedUrl(signature.SignatureUrl),
            FileName = signature.FileName,
            ContentType = signature.ContentType,
            FileSize = signature.FileSize,
            CreatedDate = signature.CreatedDate,
            ModifiedDate = signature.ModifiedDate
        };
    }
}
