namespace Application.Helpers.Runtime;

public static class MathHelpers
{
    public const int GigabyteDivider = 1_073_741_824;  // 1_073_741_824 is aggregate 1024 per conversion step (kebibyte, mebibyte and gebibyte)

    public static double ConvertToGigabytes(double number)
    {
        if (number <= 0)
        {
            return 0;
        }

        return number / GigabyteDivider;
    }

    public static ulong ConvertToGigabytes(ulong number)
    {
        if (number <= 0)
        {
            return 0;
        }

        return number / GigabyteDivider;
    }

    public static double ConvertToMbps(double number)
    {
        if (number < 1_000_000)
        {
            return 0;
        }

        return number / 1_000_000;
    }
}