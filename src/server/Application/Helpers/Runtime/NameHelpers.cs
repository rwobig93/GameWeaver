

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
        return NameGenerator.Identifiers.Get(1, IdentifierTemplate.AnyThreeComponents, separator: spaces ? " " : "").First();
    }
}