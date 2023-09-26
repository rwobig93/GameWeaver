using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
// ReSharper disable StringLiteralTypo

namespace Application.Constants.Identity;

public static class UserConstants
{
    public static readonly PasswordOptions PasswordRequirements = new()
    {
        RequiredLength = 12,
        RequiredUniqueChars = 1,
        RequireNonAlphanumeric = true,
        RequireLowercase = true,
        RequireUppercase = true,
        RequireDigit = true
    };

    public static readonly UserOptions UserRequirements = new()
    {
        AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._$@+",
        RequireUniqueEmail = true
    };

    public static readonly ClaimsIdentity UnauthenticatedIdentity = new();
    public static readonly ClaimsIdentity ExpiredIdentity = new(new[] { new Claim(ClaimTypes.Name, "ExpiredUserIdentity") });

    public static readonly ClaimsPrincipal UnauthenticatedPrincipal = new(UnauthenticatedIdentity);
    public static readonly ClaimsPrincipal ExpiredPrincipal = new(ExpiredIdentity);
    
    public static class DefaultUsers
    {
        public const string AdminUsername = "Superperson";
        public const string AdminFirstName = "Admini";
        public const string AdminLastName = "Strator";
        public const string AdminEmail = "Superperson@localhost.local";
        public const string AdminPassword = "^8yi#aFU9GstJdU9PwK9b6!&t^6hyjUg3!v^FT2cDF5mwjPGyvwfiR*";
    
        public const string ModeratorUsername = "OldGregg";
        public const string ModeratorFirstName = "Gregg";
        public const string ModeratorLastName = "Fishman";
        public const string ModeratorEmail = "OldGregg@downstairs.mixup";
        public const string ModeratorPassword = "JF9JWFeK6J6^giNw^E4nm9#9^N^PA2iQxd4yVWVU4Dyk6*iUWKa^H6!";
    
        public const string BasicUsername = "TheNeutral";
        public const string BasicFirstName = "Neutral";
        public const string BasicLastName = "Maybe";
        public const string BasicEmail = "TheNeutral@doop.future";
        public const string BasicPassword = "wFWHo^^@Lv%df$Exo7h&KWeTj35t4g3GBu^LPz9^35KCDT6A@K#zMZ3";
        
        public const string AnonymousUsername = "Anonymous";
        public const string AnonymousFirstName = "Anonymous";
        public const string AnonymousLastName = "User";
        public const string AnonymousEmail = "Who@am.i";
    
        public const string SystemUsername = "System";
        public const string SystemFirstName = "The";
        public const string SystemLastName = "System";
        public const string SystemEmail = "TheSystem@localhost.local";
    }
}