using RandomFriendlyNameGenerator;

namespace Application.Helpers.Runtime;

public static class NameHelpers
{
    public static string GeneratePassphrase(bool includeNumbers = true)
    {
        var basePassphraseOptions = NameGenerator.Identifiers.Get(15,
            IdentifierComponents.FirstName | IdentifierComponents.Adjective | IdentifierComponents.Animal, NameOrderingStyle.BobTheBuilderStyle,
            separator: "").ToList();
        var chosenOption = basePassphraseOptions.ElementAt(Random.Shared.Next(basePassphraseOptions.Count));
        var randomNumber = Random.Shared.Next(10000, 99999);
        return $"{chosenOption}{randomNumber}";
    }

    public static string GenerateName(bool spaces = false)
    {
        return Random.Shared.Next(0, 10) switch
        {
            >= 0 and <= 3 => NameGenerator.Identifiers.Get(1,
                IdentifierComponents.FirstName | IdentifierComponents.Adjective | IdentifierComponents.Animal, NameOrderingStyle.BobTheBuilderStyle,
                separator: spaces ? " " : "").First(),
            >= 4 and <= 6 => NameGenerator.Identifiers.Get(1, IdentifierTemplate.AnyThreeComponents,
                separator: spaces ? " " : "", forceSingleLetter: true).First(),
            _ => NameGenerator.Identifiers.Get(1, IdentifierTemplate.AnyThreeComponents, separator: spaces ? " " : "").First()
        };
    }
}