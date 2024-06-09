// ReSharper disable InconsistentNaming
namespace Application.Models.External.Steam;

public class SteamPriceOverviewResponseJson
{
    public string Currency { get; set; } = "";
    public int Initial { get; set; }
    public int Final { get; set; }
    public int Discount_Percent { get; set; }
    public string Initial_Formatted { get; set; } = "";
    public string Final_Formatted { get; set; } = "";
}