using CodenameGenerator;

namespace Application.Helpers.Runtime;

public static class NameHelpers
{
    private static readonly Generator GeneratorShort = new(casing: Casing.PascalCase)
    {
        Parts = [
            WordBank.Verbs,
            WordBank.Nouns
        ]
    };

    private static readonly Generator Generator = new(casing: Casing.PascalCase)
    {
        Parts = [
            WordBank.Adverbs,
            WordBank.Verbs,
            WordBank.Nouns
        ]
    };

    private static readonly Generator GeneratorLong = new(casing: Casing.PascalCase)
    {
        Parts = [
            WordBank.Adverbs,
            WordBank.Verbs,
            WordBank.Adjectives,
            WordBank.Nouns
        ]
    };

    public static string GeneratePassphrase(int numbers = 3)
    {
        var max = numbers switch
        {
            1 => 9,
            2 => 99,
            3 => 999,
            4 => 9999,
            5 => 99999,
            _ => 999999
        };
        var number = Random.Shared.Next(0, max);
        return $"{GeneratorShort.Generate().Replace(" ", "")}{number}";
    }

    public static string GenerateHostname(bool spaces = false)
    {
        var hostname = Generator.Generate();
        return spaces ? hostname : hostname.Replace(" ", "");
    }

    public static string GenerateHostnameLong(bool spaces = false)
    {
        var hostname = GeneratorLong.Generate();
        return spaces ? hostname : hostname.Replace(" ", "");
    }
}