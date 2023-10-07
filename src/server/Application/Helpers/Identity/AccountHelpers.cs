using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Application.Constants.Identity;
using Application.Helpers.Web;
using Application.Responses.v1.Identity;
using Domain.DatabaseEntities.Identity;
using Microsoft.AspNetCore.Identity;
using Bcrypt = BCrypt.Net.BCrypt;

namespace Application.Helpers.Identity;

public static class AccountHelpers
{
    public static PasswordValidator<AppUserDb> PasswordValidator { get; } = new();

    public static string GenerateClientId()
    {
        return UrlHelpers.GenerateToken(64);
    }

    private static string GenerateSalt()
    {
        return Bcrypt.GenerateSalt();
    }

    private static string GenerateHash(string password, string salt, string pepper)
    {
        return Bcrypt.HashPassword(password + pepper, salt + pepper);
    }

    public static void GenerateHashAndSalt(string password, string pepper, out string salt, out string hash)
    {
        salt = GenerateSalt();
        hash = GenerateHash(password, salt, pepper);
    }

    public static bool IsPasswordCorrect(string password, string salt, string pepper, string hash)
    {
        var newHash = GenerateHash(password, salt, pepper);
        return hash == newHash;
    }

    public static string NormalizeForDatabase(this string providedString)
    {
        return providedString.Normalize();
    }

    public static bool IsValidEmailAddress(string emailAddress, bool verifyHost = false)
    {
        try
        {
            var validEmail = new MailAddress(emailAddress);
            if (verifyHost)
            {
                Dns.GetHostEntry(validEmail.Host);
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static PasswordRequirementsResponse GetPasswordRequirements()
    {
        return new PasswordRequirementsResponse
        {
            MinimumLength = UserConstants.PasswordRequirements.RequiredLength,
            RequiresSpecialCharacters = UserConstants.PasswordRequirements.RequireNonAlphanumeric,
            RequiresLowercase = UserConstants.PasswordRequirements.RequireLowercase,
            RequiresUppercase = UserConstants.PasswordRequirements.RequireUppercase,
            RequiresNumbers = UserConstants.PasswordRequirements.RequireDigit
        };
    }

    private static bool PasswordContainsSpecialCharacter(this string password)
    {
        return (password.Any(char.IsPunctuation) || password.Any(char.IsSymbol) || password.Any(char.IsSeparator));
    }

    public static List<string> GetAnyIssuesWithPassword(string password)
    {
        var issueList = new List<string>();
        var passwordRequirements = GetPasswordRequirements();

        if (password.Length < passwordRequirements.MinimumLength)
            issueList.Add($"Password provided doesn't meet the minimum character count of {passwordRequirements.MinimumLength}");
        if (password.Length > passwordRequirements.MaximumLength)
            issueList.Add($"Password provided is over the maximum character count of {passwordRequirements.MaximumLength}");
        if (passwordRequirements.RequiresNumbers && !password.Any(char.IsDigit))
            issueList.Add("Password provided doesn't contain a number, which is a requirement");
        if (passwordRequirements.RequiresSpecialCharacters && !password.PasswordContainsSpecialCharacter())
            issueList.Add("Password provided doesn't contain a special character, which is a requirement");
        if (passwordRequirements.RequiresLowercase && !password.Any(char.IsLower))
            issueList.Add("Password provided doesn't contain a lowercase character, which is a requirement");
        if (passwordRequirements.RequiresUppercase && !password.Any(char.IsUpper))
            issueList.Add("Password provided doesn't contain an uppercase character, which is a requirement");

        return issueList;
    }

    public static bool DoesPasswordMeetRequirements(string password)
    {
        var passwordRequirements = GetPasswordRequirements();

        if (password.Length < passwordRequirements.MinimumLength)
            return false;
        if (password.Length > passwordRequirements.MaximumLength)
            return false;
        if (passwordRequirements.RequiresNumbers && !password.Any(char.IsDigit))
            return false;
        if (passwordRequirements.RequiresSpecialCharacters && !password.PasswordContainsSpecialCharacter())
            return false;
        if (passwordRequirements.RequiresLowercase && !password.Any(char.IsLower))
            return false;
        if (passwordRequirements.RequiresUppercase && !password.Any(char.IsUpper))
            return false;

        return true;
    }

    public static Guid GetId(this IEnumerable<Claim> principalClaims)
    {
        try
        {
            var rawId = principalClaims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            return Guid.Parse(rawId);
        }
        catch (Exception)
        {
            return Guid.Empty;
        }
    }
}