using Application.Services.Integrations;
using Domain.Enums.Integrations;
using OtpNet;

namespace Infrastructure.Services.Integrations;

public class MfaService : IMfaService
{
    private Totp? _totpProvider;

    public byte[] GenerateKeyBytes(int keyLength = 20)
    {
        return KeyGeneration.GenerateRandomKey(keyLength);
    }

    public string GenerateKeyString(int keyLength = 20)
    {
        return Base32Encoding.ToString(GenerateKeyBytes(keyLength));
    }

    public bool IsPasscodeCorrect(string passcode, string totpKey, out long timeStampMatched)
    {
        _totpProvider ??= new Totp(Base32Encoding.ToBytes(totpKey), step: 30, mode: OtpHashMode.Sha1, totpSize: 6);
        
        return _totpProvider.VerifyTotp(passcode, out timeStampMatched, VerificationWindow.RfcSpecifiedNetworkDelay);
    }

    private static string GetTotpAlgorithmString(TotpAlgorithm algorithm)
    {
        return algorithm switch
        {
            TotpAlgorithm.Sha1 => "SHA1",
            TotpAlgorithm.Sha256 => "SHA256",
            TotpAlgorithm.Sha512 => "SHA512",
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, "Unsupported TOTP Algorithm Provided")
        };
    }

    /// <summary>
    /// Generate TOTP authentication string for use in authenticator apps like Google Authenticator and Authy
    /// </summary>
    /// <param name="appName">Application / Issuer Name</param>
    /// <param name="accountIdentifier">Account identifier like Username or Email Address</param>
    /// <param name="totpSecret">Secret Key used to seed TOTP key generation</param>
    /// <param name="algorithm">TOTP algorithm to use | NOTE: Most providers only support SHA1</param>
    /// <param name="digits">TOTP code size in numbers to generate | NOTE: Some providers only support 6</param>
    /// <param name="secondsAlive">Time in seconds for the TOTP code generated to be valid | NOTE: Some providers only support 30</param>
    /// <returns>Generated TOTP authentication string primarily used to generate a QR Code for registering a TOTP generator</returns>
    public string GenerateOtpAuthString(string appName, string accountIdentifier, string totpSecret, TotpAlgorithm algorithm = TotpAlgorithm.Sha1,
        int digits = 6, int secondsAlive = 30)
    {
        var algorithmString = GetTotpAlgorithmString(algorithm);
        
        // TOTP Syntax: otpauth://totp/Name:email@example.com?secret=<code>&&issuer=Name&algorithm=SHA512&digits=6&period=30
        var totpAuthString =
            $"otpauth://totp/{appName}:{accountIdentifier}?secret={totpSecret}" +
            $"&issuer={appName}&algorithm={algorithmString}&digits={digits}&period={secondsAlive}";

        return totpAuthString;
    }
}