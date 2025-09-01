namespace GameWeaver.Components.Account;

public partial class AccountAvatar
{
    [Parameter] public string? Username { get; set; } = "Nobody";
    [Parameter] public string ProfileImageUrl { get; set; } = "";
    [Parameter] public Size AvatarSize { get; set; } = Size.Small;
    [Parameter] public Color AvatarColor { get; set; } = Color.Primary;
    [Parameter] public string Style { get; set; } = "";

    private string GetUsernameFromState()
    {
        return string.IsNullOrWhiteSpace(ProfileImageUrl) ? Username?[0].ToString() ?? string.Empty : string.Empty;
    }
}