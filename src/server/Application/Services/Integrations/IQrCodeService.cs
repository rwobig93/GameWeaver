namespace Application.Services.Integrations;

public interface IQrCodeService
{
    public string GenerateQrCodeSrc(string textToEncode);
}