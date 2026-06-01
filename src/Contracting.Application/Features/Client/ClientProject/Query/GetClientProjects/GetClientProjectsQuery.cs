using Contracting.Shared.Dtos.ClientDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientProject.Query.GetClientProjects;

public record GetClientProjectsQuery() : IRequest<ErrorOr<List<GetClientProjectListItemDto>>>;
