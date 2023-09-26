using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using static System.GC;

namespace Application.Helpers.Web;

public class UrlHelpers : IDisposable
{
    private RandomNumberGenerator _numberGenerator;
    private readonly int _numberOfBytes;

    public UrlHelpers(int numberOfBytes)
    {
        _numberGenerator = RandomNumberGenerator.Create();
        _numberOfBytes = numberOfBytes;
    }

    /// <summary>
    /// Generate a cryptographically secure array of bytes with a fixed length
    /// </summary>
    /// <returns></returns>
    private byte[] GenerateRandomBytes()
    {
        var byteArray = new byte[_numberOfBytes];
        _numberGenerator.GetBytes(byteArray);
        return byteArray;
    }

    public void Dispose()
    {
        SuppressFinalize(this);
        _numberGenerator.Dispose();
        _numberGenerator = null!;
    }

    private string GenerateToken()
    {
        return WebEncoders.Base64UrlEncode(GenerateRandomBytes());
    }

    /// <summary>
    /// Generate a single fixed length token that can be used in a URL
    /// </summary>
    /// <param name="numberOfBytes">Desired token size in bytes, larger is more secure</param>
    /// <returns>A single generated URL friendly token</returns>
    /// <exception cref="ArgumentOutOfRangeException">Byte size can be 1 to 256</exception>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static string GenerateToken(int numberOfBytes = 32)
    {
        if (numberOfBytes is <= 0 or > 256) throw new ArgumentOutOfRangeException(nameof(numberOfBytes));
        return GenerateTokens(1, numberOfBytes).First();
    }

    /// <summary>
    /// Generate a desired count of fixed length tokens that can be used in a URL
    /// </summary>
    /// <param name="tokenCount">Number of tokens to generate and return</param>
    /// <param name="numberOfBytes">Desired token size in bytes, larger is more secure</param>
    /// <returns>An enumerable of generated URL friendly tokens</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<string> GenerateTokens(int tokenCount, int numberOfBytes = 32)
    {
        using var factory = new UrlHelpers(numberOfBytes);
        while (tokenCount > 0)
        {
            tokenCount--;
            yield return factory.GenerateToken();
        }
    }

    public static string SanitizeTextForUrl(string textToSanitize)
    {
        return WebUtility.UrlEncode(textToSanitize);
    }
}