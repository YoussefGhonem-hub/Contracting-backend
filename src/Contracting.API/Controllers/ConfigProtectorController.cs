using Contracting.API.Controllers.Shared;
using Contracting.Shared.Constants;
using Contracting.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    /// <summary>
    /// Utility endpoints for encrypting/decrypting sensitive appsettings.json values
    /// (connection strings, AWS keys) before they're published to a server. Not part of
    /// the app's normal runtime flow — used by an admin, from a trusted machine, to
    /// produce the "ENC:..." ciphertext that goes into appsettings.json.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public class ConfigProtectorController : APIBaseController
    {
        private readonly IConfiguration _configuration;

        public ConfigProtectorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetKey()
        {
            var key = _configuration["Encryption:Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Missing 'Encryption:Key' in appsettings.");
            return key;
        }

        public record EncryptRequestDto(string PlainText);
        public record DecryptRequestDto(string CipherText);

        // Encrypt a plain value (e.g. a connection string) using the key configured in appsettings
        // under "Encryption:Key". Paste the returned CipherText into appsettings.json before publishing.
        [HttpPost("encrypt")]
        public IActionResult Encrypt([FromBody] EncryptRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.PlainText))
                return BadRequest(new { message = "PlainText is required." });

            var cipherText = AppSettingsProtector.Encrypt(dto.PlainText, GetKey());
            return Ok(new { cipherText });
        }

        // Decrypt a value previously produced by /encrypt. Mainly useful to verify a value
        // before publishing, or to recover a value already stored in appsettings.json.
        [HttpPost("decrypt")]
        public IActionResult Decrypt([FromBody] DecryptRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.CipherText))
                return BadRequest(new { message = "CipherText is required." });

            if (!AppSettingsProtector.IsEncrypted(dto.CipherText))
                return BadRequest(new { message = "Value is not in the expected 'ENC:' encrypted format." });

            var plainText = AppSettingsProtector.Decrypt(dto.CipherText, GetKey());
            return Ok(new { plainText });
        }

        // Generates a fresh random key for the "Encryption:Key" appsetting. Only needed once,
        // or when rotating the key (which requires re-encrypting all existing ENC: values with it).
        [HttpGet("generate-key")]
        public IActionResult GenerateKey()
        {
            return Ok(new { key = AppSettingsProtector.GenerateKey() });
        }
    }
}
