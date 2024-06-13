// ReSharper disable InconsistentNaming
namespace Application.Models.External.Steam;

public class SteamAppDetailResponseJson
{
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public int Steam_AppId { get; set; }
    public int Required_Age { get; set; }
    public bool Is_Free { get; set; }
    public string Controller_Support { get; set; } = "";
    public List<int> Dlc { get; set; } = [];
    public string Detailed_Description { get; set; } = "";
    public string About_The_Game { get; set; } = "";
    public string Short_Description { get; set; } = "";
    public string Header_Image { get; set; } = "";
    public SteamHardwareRequirementsResponseJson PC_Requirements { get; set; } = new();
    public SteamHardwareRequirementsResponseJson Mac_Requirements { get; set; } = new();
    public SteamHardwareRequirementsResponseJson Linux_Requirements { get; set; } = new();
    public List<string> Developers { get; set; } = [];
    public List<string> Publishers { get; set; } = [];
    public SteamPriceOverviewResponseJson Price_Overview { get; set; } = new();
    public SteamPlatformsResponseJson Platforms { get; set; } = new();
    public List<SteamGenreResponseJson> Genres { get; set; } = [];
    public string Background_Raw { get; set; } = "";
}