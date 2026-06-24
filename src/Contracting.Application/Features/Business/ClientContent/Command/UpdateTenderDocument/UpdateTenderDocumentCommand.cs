using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Contracting.Application.Features.Business.ClientContent.Command.UpdateTenderDocument;

public record UpdateTenderDocumentCommand(
    Guid TenderId,
    string? Title,
    IFormFile? File) : IRequest<ErrorOr<UpdatedTenderDocumentDto>>;
