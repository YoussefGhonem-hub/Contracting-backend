using Contracting.Shared.Dtos.ClientDtos.TenderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Tender.Query.GetClientTenderDocuments;

public record GetClientTenderDocumentsQuery(Guid ProjectId) : IRequest<ErrorOr<List<GetClientTenderDocumentDto>>>;
