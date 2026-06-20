using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.ClientManagement.Command.CreateClient;
using Contracting.Application.Features.Client.ClientManagement.Command.DeleteClient;
using Contracting.Application.Features.Client.ClientManagement.Command.UpdateClient;
using Contracting.Application.Features.Client.ClientManagement.Query.GetClientById;
using Contracting.Application.Features.Client.ClientManagement.Query.GetClientList;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientController : APIBaseController
    {
        // Roles allowed to mutate client records (create/update/delete).
        private const string ManageRoles = RoleNames.SuperAdmin + "," + RoleNames.Admin + "," + RoleNames.IT + "," + RoleNames.Accounts;

        // Roles allowed to read client details — back-office plus the engineer roles
        // that need to view a project's client information.
        private const string ReadRoles = ManageRoles
            + "," + RoleNames.Teamleadengineer
            + "," + RoleNames.Siteengineer
            + "," + RoleNames.Officeengineer;

        private readonly IMediator _mediator;

        public ClientController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Client (creates login user + Client role + profile)
        [HttpPost]
        [Authorize(Roles = ManageRoles)]
        public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
        {
            var result = await _mediator.Send(new CreateClientCommand(dto));
            return result.Match(client => Ok(client), errors => Problem(errors));
        }

        // Update Client
        [HttpPut]
        [Authorize(Roles = ManageRoles)]
        public async Task<IActionResult> Update([FromBody] UpdateClientDto dto)
        {
            var result = await _mediator.Send(new UpdateClientCommand(dto));
            return result.Match(client => Ok(client), errors => Problem(errors));
        }

        // Delete Client (removes profile, project links, and login user)
        [HttpDelete("{clientId:guid}")]
        [Authorize(Roles = ManageRoles)]
        public async Task<IActionResult> Delete(Guid clientId)
        {
            var result = await _mediator.Send(new DeleteClientCommand(clientId));
            return result.Match(success => Ok(success), errors => Problem(errors));
        }

        // Get Clients (paginated)
        [HttpGet]
        [Authorize(Roles = ReadRoles)]
        public async Task<IActionResult> GetAll([FromQuery] BaseFilterDto filter)
        {
            var result = await _mediator.Send(new GetClientListQuery(filter));
            return result.Match(clients => Ok(clients), errors => Problem(errors));
        }

        // Get Client By Id (readable by back-office + engineer roles viewing a project's client)
        [HttpGet("{clientId:guid}")]
        [Authorize(Roles = ReadRoles)]
        public async Task<IActionResult> GetById(Guid clientId)
        {
            var result = await _mediator.Send(new GetClientByIdQuery(clientId));
            return result.Match(client => Ok(client), errors => Problem(errors));
        }
    }
}
