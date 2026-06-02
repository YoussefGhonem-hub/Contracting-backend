using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Invoice.Query.GetClientFinancialSummary;

public class GetClientFinancialSummaryQueryHandler : IRequestHandler<GetClientFinancialSummaryQuery, ErrorOr<GetClientFinancialSummaryDto>>
{
    private readonly IClientInvoiceService _service;

    public GetClientFinancialSummaryQueryHandler(IClientInvoiceService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientFinancialSummaryDto>> Handle(GetClientFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetClientFinancialSummaryAsync(request.ProjectId, cancellationToken: cancellationToken);

        if (result is null)
            return Error.NotFound("Invoice.ProjectNotFound", "Project not found or not assigned to this client.");

        return result;
    }
}
