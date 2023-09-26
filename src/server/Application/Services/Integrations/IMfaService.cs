using Domain.Enums.Integration;

namespace Application.Services.Integrations;

public interface IMfaService
{
    public byte[] GenerateKeyBytes(int keyLength = 20);
    public string GenerateKeyString(int keyLength = 20);
    public bool IsPasscodeCorrect(string passcode, string totpKey, out long timeStampMatched);

    public string GenerateOtpAuthString(string appName, string accountIdentifier, string totpSecret,
        TotpAlgorithm algorithm = TotpAlgorithm.Sha1, int digits = 6, int secondsAlive = 30);
}