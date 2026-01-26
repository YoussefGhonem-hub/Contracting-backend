using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Application.Features.Users.Queries.GetUserSignature;

public record GetUserSignatureQuery : IRequest<ErrorOr<UserSignatureDto>>;

public class GetUserSignatureQueryHandler : IRequestHandler<GetUserSignatureQuery, ErrorOr<UserSignatureDto>>
{
    private readonly ApplicationDbContext _db;

    public GetUserSignatureQueryHandler(ApplicationDbContext db)
    {
        _db = db;
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
            SignatureUrl = signature.SignatureUrl,
            FileName = signature.FileName,
            ContentType = signature.ContentType,
            FileSize = signature.FileSize,
            CreatedDate = signature.CreatedDate,
            ModifiedDate = signature.ModifiedDate
        };
    }
}
