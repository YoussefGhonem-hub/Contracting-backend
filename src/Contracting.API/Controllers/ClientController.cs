using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.ClientManagement.Command.CreateClient;
using Contracting.Application.Features.Client.ClientManagement.Command.DeleteClient;
using Contracting.Application.Features.Client.ClientManagement.Command.UpdateClient;
using Contracting.Application.Features.Client.ClientManagement.Query.GetClientById;
using Contracting.Application.Features.Client.ClientManagement.Query.GetClientList;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin,IT")]
    public class ClientController : APIBaseController
    {
        private readonly IMediator _mediator;

        public ClientController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Client (creates login user + Client role + profile)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
        {
            var result = await _mediator.Send(new CreateClientCommand(dto));
            return result.Match(client => Ok(client), errors => Problem(errors));
        }

        // Update Client
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateClientDto dto)
        {
            var result = await _mediator.Send(new UpdateClientCommand(dto));
            return result.Match(client => Ok(client), errors => Problem(errors));
        }

        // Delete Client (removes profile, project links, and login user)
        [HttpDelete("{clientId:guid}")]
        public async Task<IActionResult> Delete(Guid clientId)
        {
            var result = await _mediator.Send(new DeleteClientCommand(clientId));
            return result.Match(success => Ok(success), errors => Problem(errors));
        }

        // Get Clients (paginated)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BaseFilterDto filter)
        {
            var result = await _mediator.Send(new GetClientListQuery(filter));
            return result.Match(clients => Ok(clients), errors => Problem(errors));
        }

        // Get Client By Id
        [HttpGet("{clientId:guid}")]
        public async Task<IActionResult> GetById(Guid clientId)
        {
            var result = await _mediator.Send(new GetClientByIdQuery(clientId));
            return result.Match(client => Ok(client), errors => Problem(errors));
        }
    }
}
