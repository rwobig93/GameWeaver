namespace GameServerQuery.Parsers.Helpers;

public static class PayloadHelpers
{
    public static bool RemainingIsEmpty(this byte[] payload, int start, int end = -1)
    {
        if (end == -1)
        {
            end = payload.Length;
        }

        var remainingPayload = payload.Skip(start);

        return remainingPayload.Any(position => position != 0x00);
    }
}