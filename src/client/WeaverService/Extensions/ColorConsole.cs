namespace WeaverService.Extensions;

public static class ColorConsole
{
    public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;

    public static void WriteLine(string writeText, ConsoleColor color, bool addLineBreak = false)
    {
        if (addLineBreak)
            writeText += Environment.NewLine;
        Console.ForegroundColor = color;
        Console.WriteLine(writeText);
        Console.ForegroundColor = DefaultColor;
    }
}
