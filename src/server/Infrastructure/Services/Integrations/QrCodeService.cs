using Application.Services.Integrations;
using QRCoder;

namespace Infrastructure.Services.Integrations;

public class QrCodeService : IQrCodeService
{
    private readonly QRCodeGenerator _codeGenerator;

    public QrCodeService()
    {
        _codeGenerator = new QRCodeGenerator();
    }

    public string GenerateQrCodeSrc(string textToEncode)
    {
        var qrCodeData = _codeGenerator.CreateQrCode(textToEncode, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new BitmapByteQRCode(qrCodeData);
        // NOTE: There is an ability to add an encoded image to the center of the QRCode, haven't spent time adding this though
        var qrCodeImage = qrCode.GetGraphic(20);

        return "data:image/png;base64, " + Convert.ToBase64String(qrCodeImage);

    }
}