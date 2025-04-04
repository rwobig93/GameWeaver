using CodenameGenerator;

namespace Application.Helpers.Runtime;

public static class NameHelpers
{
    private static readonly Generator Generator = new(casing: Casing.PascalCase)
    {
        Parts = [
            WordBank.Adverbs,
            WordBank.Verbs,
            WordBank.Adjectives,
            WordBank.Nouns
        ]
    };

    public static string GenerateHostname()
    {
        return Generator.Generate();
    }
}