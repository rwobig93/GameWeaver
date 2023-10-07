namespace Application.Responses.v1.Identity;

public class PasswordRequirementsResponse
{
    public int MinimumLength { get; set; }
    public int MaximumLength { get; set; } = 100;
    public bool RequiresSpecialCharacters { get; set; }
    public bool RequiresLowercase { get; set; }
    public bool RequiresUppercase { get; set; }
    public bool RequiresNumbers { get; set; }
}