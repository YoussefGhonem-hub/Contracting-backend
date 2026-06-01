using Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Invoice.Query.GetClientFinancialSummary;

public record GetClientFinancialSummaryQuery(Guid ProjectId) : IRequest<ErrorOr<GetClientFinancialSummaryDto>>;
