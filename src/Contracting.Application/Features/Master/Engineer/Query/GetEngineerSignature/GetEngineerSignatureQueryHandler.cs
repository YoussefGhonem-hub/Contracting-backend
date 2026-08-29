using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos.HelperDtos;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Storage.AWS3.Services;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerSignature
{
    public class GetEngineerSignatureQueryHandler : IRequestHandler<GetEngineerSignatureQuery, ErrorOr<UserSignatureDto>>
    {
        private readonly ApplicationDbContext _db;
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly IStorageService _s3;

        public GetEngineerSignatureQueryHandler(ApplicationDbContext db, IStringLocalizer<SharedResources> localizer, IStorageService s3)
        {
            _db = db;
            _localizer = localizer;
            _s3 = s3;
        }

        public async Task<ErrorOr<UserSignatureDto>> Handle(GetEngineerSignatureQuery request, CancellationToken cancellationToken)
        {
            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.EngineerId, cancellationToken);

            if (engineer is null)
                return Error.NotFound("Engineer.NotFound", _localizer[SharedResourcesKeys.EngineerNotFound]);

            var signature = await _db.UserSignatures
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == engineer.ApplicationUserId, cancellationToken);

            if (signature is null)
                return Error.NotFound("Signature.NotFound", "This engineer has not uploaded a signature yet.");

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
}
