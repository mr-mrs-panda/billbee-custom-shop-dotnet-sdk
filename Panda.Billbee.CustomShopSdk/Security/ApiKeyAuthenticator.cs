using System.Security.Cryptography;
using System.Text;

namespace Panda.Billbee.CustomShopSdk.Security;

/// <summary>
/// Provides HMAC-SHA256 based validation of Billbee API keys to secure requests against replay attacks.
/// </summary>
public static class ApiKeyAuthenticator
{
    /// <summary>
    /// Validates the provided API key against the current timestamp using HMAC-SHA256.
    /// </summary>
    /// <param name="plainApiKey">Shared API key configured in the shop system.</param>
    /// <param name="receivedKey">Encrypted key parameter sent by Billbee for authentication.</param>
    /// <returns>True if the key is valid and within the allowed time window; otherwise false.</returns>
    public static bool Validate(string plainApiKey, string receivedKey)
    {
        if (string.IsNullOrEmpty(plainApiKey) || string.IsNullOrEmpty(receivedKey))
            return false;

        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampStr = currentTimestamp.ToString()[..7];

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(timestampStr));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainApiKey));
        var hexString = Convert.ToHexString(hash).ToLowerInvariant();
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(hexString));
        var sanitized = base64.Replace("=", string.Empty)
            .Replace("/", string.Empty)
            .Replace("+", string.Empty);

        return string.Equals(sanitized, receivedKey, StringComparison.Ordinal);
    }

    /// <summary>
    /// Validates Basic Authentication credentials.
    /// </summary>
    /// <param name="expectedUsername">The expected username.</param>
    /// <param name="expectedPassword">The expected password.</param>
    /// <param name="authHeader">The Authorization header in the format "Basic base64(username:password)".</param>
    /// <returns>True if the credentials are valid; otherwise false.</returns>
    public static bool ValidateBasicAuth(string expectedUsername, string expectedPassword, string? authHeader)
    {
        if (string.IsNullOrEmpty(expectedUsername) || string.IsNullOrEmpty(expectedPassword))
            return true;

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            var base64Credentials = authHeader["Basic ".Length..].Trim();
            var credentialsBytes = Convert.FromBase64String(base64Credentials);
            var credentials = Encoding.UTF8.GetString(credentialsBytes).Split(':', 2);

            if (credentials.Length != 2)
                return false;

            var username = credentials[0];
            var password = credentials[1];

            return string.Equals(username, expectedUsername, StringComparison.Ordinal) &&
                   string.Equals(password, expectedPassword, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}