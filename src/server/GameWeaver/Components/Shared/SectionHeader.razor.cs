namespace GameWeaver.Components.Shared;

public partial class SectionHeader : ComponentBase
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string TitleClass { get; set; } = "pa-1";
    [Parameter] public Typo TextTypo { get; set; } = Typo.h6;
    [Parameter] public Color Color { get; set; } = Color.Primary;
    [Parameter] public string Class { get; set; } = "mb-0";
    [Parameter] public string Style { get; set; } = string.Empty;
    [Parameter] public bool GamerMode { get; set; }

    private string GetBorderClass()
    {
        if (GamerMode)
        {
            return $"border-rainbow {Class}";
        }

        var borderType = Color switch
        {
            Color.Primary => "section-header-primary",
            Color.Secondary => "section-header-secondary",
            Color.Tertiary => "section-header-tertiary",
            _ => "section-header-primary"
        };

        return string.IsNullOrWhiteSpace(Class) ? borderType : $"{borderType} {Class}";
    }

    private string GetTextClass()
    {
        return GamerMode ? "rainbow-text" : "";
    }
}