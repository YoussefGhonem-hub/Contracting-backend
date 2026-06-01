using Contracting.Domain.Common.Enums;

namespace Contracting.Infrustructure.Inteface.client;

public interface IFirebaseService
{
    /// <summary>
    /// Generates a Firebase custom token for the given user so the client app can authenticate
    /// with Firebase and subscribe to Firestore real-time updates.
    /// </summary>
    Task<string> GenerateCustomTokenAsync(string uid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes a new chat message to Firestore so connected clients receive it in real time.
    /// </summary>
    Task PushMessageAsync(FirestoreChatMessage message, CancellationToken cancellationToken = default);
}

public record FirestoreChatMessage(
    string MessageId,
    string ChatGroupId,
    string SenderId,
    string SenderName,
    string? Content,
    ChatMessageType MessageType,
    string? AttachmentUrl,
    string? AttachmentFileName,
    long? AttachmentFileSize,
    DateTimeOffset SentAt
);
