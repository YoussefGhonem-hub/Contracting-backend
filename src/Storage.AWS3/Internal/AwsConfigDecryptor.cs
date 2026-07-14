using System.Security.Cryptography;
using System.Text;

namespace Storage.AWS3.Internal;

/// <summary>
/// Decrypts "ENC:..." AWS credential values from appsettings.json. Mirrors
/// Contracting.Shared.Security.AppSettingsProtector's format exactly (AES-256-GCM,
/// nonce || tag || ciphertext, Base64) — duplicated here rather than referenced because
/// Storage.AWS3 has no project reference to Contracting.Shared. Values are encrypted via
/// the ConfigProtector API (which uses AppSettingsProtector), so the two must stay in sync.
/// </summary>
internal static class AwsConfigDecryptor
{
    private const string Prefix = "ENC:";
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static bool IsEncrypted(string? value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);

    public static string Decrypt(string value, string base64Key)
    {
        if (!IsEncrypted(value))
            return value;

        var payload = Convert.FromBase64String(value[Prefix.Length..]);
        var key = Convert.FromBase64String(base64Key);

        var nonce = payload[..NonceSize];
        var tag = payload[NonceSize..(NonceSize + TagSize)];
        var cipherBytes = payload[(NonceSize + TagSize)..];
        var plainBytes = new byte[cipherBytes.Length];

        using (var aesGcm = new AesGcm(key, TagSize))
        {
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }
}
