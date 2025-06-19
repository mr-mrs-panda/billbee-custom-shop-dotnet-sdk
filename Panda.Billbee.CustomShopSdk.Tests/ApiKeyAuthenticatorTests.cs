using System.Security.Cryptography;
using System.Text;
using Panda.Billbee.CustomShopSdk.Security;

namespace Panda.Billbee.CustomShopSdk.Tests;

public class ApiKeyAuthenticatorTests
{
    [Fact]
    public void Validate_WithNullPlainApiKey_ReturnsFalse()
    {
        // Arrange
        string? plainApiKey = null;
        string receivedKey = "somekey";

        // Act
        var result = ApiKeyAuthenticator.Validate(plainApiKey!, receivedKey);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithEmptyPlainApiKey_ReturnsFalse()
    {
        // Arrange
        string plainApiKey = string.Empty;
        string receivedKey = "somekey";

        // Act
        var result = ApiKeyAuthenticator.Validate(plainApiKey, receivedKey);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithNullReceivedKey_ReturnsFalse()
    {
        // Arrange
        string plainApiKey = "myapikey";
        string? receivedKey = null;

        // Act
        var result = ApiKeyAuthenticator.Validate(plainApiKey, receivedKey!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithEmptyReceivedKey_ReturnsFalse()
    {
        // Arrange
        string plainApiKey = "myapikey";
        string receivedKey = string.Empty;

        // Act
        var result = ApiKeyAuthenticator.Validate(plainApiKey, receivedKey);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithValidKeyGeneratedProgrammatically_ReturnsTrue()
    {
        // Arrange
        string plainApiKey = "test-api-key-123";
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampStr = currentTimestamp.ToString()[..7];

        // Generate the expected key using the same algorithm as the authenticator
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(timestampStr));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainApiKey));
        var hexString = Convert.ToHexString(hash).ToLowerInvariant();
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(hexString));
        var expectedKey = base64.Replace("=", string.Empty)
            .Replace("/", string.Empty)
            .Replace("+", string.Empty);

        // Act
        var result = ApiKeyAuthenticator.Validate(plainApiKey, expectedKey);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Validate_WithInvalidKey_ReturnsFalse()
    {
        // Arrange
        string plainApiKey = "test-api-key-123";
        string receivedKey = "invalid-key";

        // Act
        var result = ApiKeyAuthenticator.Validate(plainApiKey, receivedKey);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithDifferentApiKeys_ReturnsFalse()
    {
        // Arrange
        string plainApiKey1 = "test-api-key-123";
        string plainApiKey2 = "different-api-key-456";
        
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampStr = currentTimestamp.ToString()[..7];

        // Generate key for plainApiKey1
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(timestampStr));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainApiKey1));
        var hexString = Convert.ToHexString(hash).ToLowerInvariant();
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(hexString));
        var keyForApiKey1 = base64.Replace("=", string.Empty)
            .Replace("/", string.Empty)
            .Replace("+", string.Empty);

        // Act - validate with different API key
        var result = ApiKeyAuthenticator.Validate(plainApiKey2, keyForApiKey1);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(null, "")]
    public void ValidateBasicAuth_WithNullOrEmptyCredentials_ReturnsTrue(string? username, string? password)
    {
        // Arrange
        string authHeader = "Basic dGVzdDp0ZXN0"; // test:test

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username!, password!, authHeader);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithNullAuthHeader_ReturnsFalse()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        string? authHeader = null;

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithEmptyAuthHeader_ReturnsFalse()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        string authHeader = string.Empty;

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithoutBasicPrefix_ReturnsFalse()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        string authHeader = "Bearer token123";

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithValidCredentials_ReturnsTrue()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        string authHeader = $"Basic {credentials}";

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithValidCredentialsCaseInsensitivePrefix_ReturnsTrue()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        string authHeader = $"basic {credentials}"; // lowercase

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithWrongUsername_ReturnsFalse()
    {
        // Arrange
        string expectedUsername = "testuser";
        string expectedPassword = "testpass";
        string actualUsername = "wronguser";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{actualUsername}:{expectedPassword}"));
        string authHeader = $"Basic {credentials}";

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(expectedUsername, expectedPassword, authHeader);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        string expectedUsername = "testuser";
        string expectedPassword = "testpass";
        string actualPassword = "wrongpass";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{expectedUsername}:{actualPassword}"));
        string authHeader = $"Basic {credentials}";

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(expectedUsername, expectedPassword, authHeader);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithInvalidBase64_ReturnsFalse()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        string authHeader = "Basic invalid-base64!@#";

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithMissingColon_ReturnsFalse()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testusertestpass")); // no colon
        string authHeader = $"Basic {credentials}";

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithMultipleColons_ReturnsTrue()
    {
        // Arrange
        string username = "testuser";
        string password = "test:pass:with:colons";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        string authHeader = $"Basic {credentials}";

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateBasicAuth_WithWhitespaceInHeader_ReturnsTrue()
    {
        // Arrange
        string username = "testuser";
        string password = "testpass";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        string authHeader = $"Basic   {credentials}   "; // extra whitespace

        // Act
        var result = ApiKeyAuthenticator.ValidateBasicAuth(username, password, authHeader);

        // Assert
        Assert.True(result);
    }
}