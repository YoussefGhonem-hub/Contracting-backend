using System.Security.Cryptography;
using System.Text;

namespace Contracting.Shared.Security;

/// <summary>
/// AES-256-GCM encryption for sensitive appsettings.json values (connection strings, AWS keys, etc.).
/// Ciphertext is always prefixed with <see cref="Prefix"/> so <see cref="Decrypt"/> can tell an
/// encrypted value apart from a plain one left as-is for local development — mixing encrypted
/// production secrets and plaintext dev values in the same appsettings.json shape is intentional.
/// </summary>
public static class AppSettingsProtector
{
    private const string Prefix = "ENC:";
    private const int NonceSize = 12;
    private const int TagSize = 16;

    /// <summary>Generates a new random 256-bit key, Base64-encoded, for the "Encryption:Key" appsetting.</summary>
    public static string GenerateKey()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(key);
    }

    public static string Encrypt(string plainText, string base64Key)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var key = Convert.FromBase64String(base64Key);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using (var aesGcm = new AesGcm(key, TagSize))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        // Layout: nonce || tag || ciphertext
        var payload = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, payload, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, payload, NonceSize + TagSize, cipherBytes.Length);

        return Prefix + Convert.ToBase64String(payload);
    }

    /// <summary>
    /// Decrypts a value produced by <see cref="Encrypt"/>. Values without the "ENC:" prefix are
    /// returned unchanged, so plaintext values (e.g. local dev connection strings) keep working.
    /// </summary>
    public static string Decrypt(string value, string base64Key)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
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

    public static bool IsEncrypted(string? value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);
}
