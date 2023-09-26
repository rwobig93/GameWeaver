namespace GameWeaver.Pages.Example;

public partial class Counter
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    private int _currentCount;
    private Color _iconColor = Color.Primary;

    private void IncrementClick()
    {
        _currentCount++;
        _iconColor = GetRandomColor();
    }

    private static Color GetRandomColor()
    {
        var colors = Enum.GetValues<Color>();
        var random = new Random();
        return (Color)colors.GetValue(random.Next(colors.Length))!;
    }
}