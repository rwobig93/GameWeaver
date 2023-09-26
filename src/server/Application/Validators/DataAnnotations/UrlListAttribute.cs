using System.ComponentModel.DataAnnotations;

namespace Application.Validators.DataAnnotations;

public class UrlListAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<string> urlList)
        {
            return ValidationResult.Success!;
        }

        foreach (var url in urlList)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return new ValidationResult($"The value '{url}' is not a valid URL.");
            }
        }

        return ValidationResult.Success!;
    }
}
