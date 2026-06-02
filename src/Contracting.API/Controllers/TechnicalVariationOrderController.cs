using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.VariationOrder.Command.CreateVariationOrder;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos.BusinessDtos.VariationOrderDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/technical/variation-orders")]
[ApiController]
[Authorize(Roles = RoleNames.Officeengineer + "," + RoleNames.Teamleadengineer)]
public class TechnicalVariationOrderController : APIBaseController
{
    private readonly IMediator _mediator;

    public TechnicalVariationOrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a variation order for a project. Technical Office only.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateVariationOrderDto dto)
    {
        var result = await _mediator.Send(new CreateVariationOrderCommand(dto));
        return result.Match(Ok, Problem);
    }
}
