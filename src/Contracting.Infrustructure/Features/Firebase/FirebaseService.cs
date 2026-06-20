using Contracting.Infrustructure.Inteface.client;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Contracting.Infrustructure.Features.Firebase;

public class FirebaseService : IFirebaseService
{
    private readonly FirebaseOptions _options;
    private readonly FirestoreDb? _firestoreDb;
    private readonly ILogger<FirebaseService> _logger;

    public FirebaseService(IOptions<FirebaseOptions> options, ILogger<FirebaseService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var credentialJson = BuildServiceAccountJson(_options);
        var credential = GoogleCredential.FromJson(credentialJson);

        // Initialize FirebaseApp once (idempotent)
        try
        {
            if (FirebaseApp.DefaultInstance is null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _options.ProjectId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FirebaseApp");
        }

        // Build Firestore using explicit JSON credentials (avoids needing Application Default Credentials)
        try
        {
            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = _options.ProjectId,
                JsonCredentials = credentialJson
            }.Build();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build FirestoreDb");
        }
    }

    public async Task<string> GenerateCustomTokenAsync(string uid, CancellationToken cancellationToken = default)
    {
        var auth = FirebaseAuth.DefaultInstance;
        if (auth is null)
            throw new InvalidOperationException("FirebaseAuth is not initialized. Check Firebase configuration in appsettings.");

        return await auth.CreateCustomTokenAsync(uid, cancellationToken: cancellationToken);
    }

    public async Task PushMessageAsync(FirestoreChatMessage message, CancellationToken cancellationToken = default)
    {
        if (_firestoreDb is null)
        {
            _logger.LogWarning("FirestoreDb not initialized — skipping push for message {MessageId}", message.MessageId);
            return;
        }

        var collection = _firestoreDb.Collection("chats")
            .Document(message.ChatGroupId)
            .Collection("messages");

        var document = new Dictionary<string, object?>
        {
            ["id"] = message.MessageId,
            ["chatGroupId"] = message.ChatGroupId,
            ["senderId"] = message.SenderId,
            ["senderName"] = message.SenderName,
            ["content"] = message.Content ?? string.Empty,
            ["messageType"] = message.MessageType.ToString(),
            ["attachmentUrl"] = message.AttachmentUrl,
            ["attachmentFileName"] = message.AttachmentFileName,
            ["attachmentFileSize"] = message.AttachmentFileSize,
            ["sentAt"] = message.SentAt.UtcDateTime
        };

        await collection.Document(message.MessageId).SetAsync(document!, cancellationToken: cancellationToken);
    }

    private static string BuildServiceAccountJson(FirebaseOptions o)
    {
        var obj = new
        {
            type = o.Type ?? "service_account",
            project_id = o.ProjectId,
            private_key_id = o.PrivateKeyId,
            private_key = o.PrivateKey,
            client_email = o.ClientEmail,
            client_id = o.ClientId,
            auth_uri = o.AuthUri,
            token_uri = o.TokenUri,
            auth_provider_x509_cert_url = o.AuthProviderX509CertUrl,
            client_x509_cert_url = o.ClientX509CertUrl,
            universe_domain = o.UniverseDomain ?? "googleapis.com"
        };
        return JsonSerializer.Serialize(obj);
    }
}
