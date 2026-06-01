using Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Invoice.Query.GetClientInvoices;

public record GetClientInvoicesQuery(Guid ProjectId, string? Status) : IRequest<ErrorOr<GetClientInvoicesDto>>;
