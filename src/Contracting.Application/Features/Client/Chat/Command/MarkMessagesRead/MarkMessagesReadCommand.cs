using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.MarkMessagesRead;

public record MarkMessagesReadCommand(Guid GroupId) : IRequest<ErrorOr<bool>>;
