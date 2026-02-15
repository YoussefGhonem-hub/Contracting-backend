using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Command.DeleteConstructionItem
{
    public record DeleteConstructionItemCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
}
