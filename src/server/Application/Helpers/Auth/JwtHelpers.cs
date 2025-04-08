using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Constants.Identity;
using Application.Services.System;
using Application.Settings.AppSettings;
using Microsoft.IdentityModel.Tokens;

namespace Application.Helpers.Auth;

public static class JwtHelpers
{
    public static readonly JwtSecurityTokenHandler JwtHandler = new();
    public const int JwtTokenValidBeforeSeconds = 3;

    private static byte[] GetJwtSecret(SecurityConfiguration securityConfig)
    {
        return Encoding.ASCII.GetBytes(securityConfig.JsonTokenSecret);
    }

    public static string GetJwtIssuer(AppConfiguration appConfig)
    {
        return appConfig.BaseUrl;
    }

    private static string GetJwtAudience(AppConfiguration appConfig)
    {
        return $"{appConfig.ApplicationName} - Users";
    }

    public static JwtSecurityToken GetJwtDecoded(string token)
    {
        return JwtHandler.ReadJwtToken(token);
    }

    public static Guid GetJwtUserId(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Guid.Empty;
        }

        try
        {
            var decodedJwt = GetJwtDecoded(token);
            var userId = decodedJwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;

            return Guid.Parse(userId);
        }
        catch
        {
            return Guid.Empty;
        }
    }

    private static DateTime GetJwtValidBeforeTime(IDateTimeService dateTime)
    {
        return dateTime.NowDatabaseTime.AddSeconds(-JwtTokenValidBeforeSeconds);
    }

    private static DateTime GetUserJwtExpirationTime(IDateTimeService dateTime, SecurityConfiguration securityConfig)
    {
        return dateTime.NowDatabaseTime.AddMinutes(securityConfig.UserTokenExpirationMinutes);
    }

    public static DateTime GetApiJwtExpirationTime(IDateTimeService dateTime, SecurityConfiguration securityConfig)
    {
        return dateTime.NowDatabaseTime.AddMinutes(securityConfig.ApiTokenExpirationMinutes);
    }

    public static DateTime GetJwtRefreshTokenExpirationTime(IDateTimeService dateTime, SecurityConfiguration securityConfig)
    {
        // Add additional buffer for refresh token to be used, since refresh JWT is used automatically Idle applies to this calculation
        return dateTime.NowDatabaseTime.AddMinutes(securityConfig.UserTokenExpirationMinutes + securityConfig.SessionIdleTimeoutMinutes);
    }

    public static DateTime GetJwtExpirationTime(string token)
    {
        var decodedJwt = GetJwtDecoded(token);
        var tokenExpirationRaw = decodedJwt.Claims.FirstOrDefault(x => x.Type == "exp")!.Value;

        var tokenExpirationParsed = DateTimeOffset.FromUnixTimeSeconds(long.Parse(tokenExpirationRaw)).UtcDateTime;

        return tokenExpirationParsed;
    }

    private static TokenValidationParameters GetJwtValidationParameters(byte[] jwtSecretKey, string issuer, string audience)
    {
        return new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSecretKey),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            LifetimeValidator = (nbf, exp, _, _) => nbf < DateTime.UtcNow && exp > DateTime.UtcNow,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name,
            ClockSkew = TimeSpan.FromSeconds(5)
        };
    }

    private static SigningCredentials GetSigningCredentials(SecurityConfiguration securityConfig)
    {
        var secret = GetJwtSecret(securityConfig);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }

    public static string GenerateUserJwtEncryptedToken(IEnumerable<Claim> claims, IDateTimeService dateTime, SecurityConfiguration securityConfig,
        AppConfiguration appConfig)
    {
        var updatedClaims = claims.ToList();
        updatedClaims.Add(new Claim(ClaimConstants.AuthenticationType, ClaimConstants.AuthType.User));

        var token = new JwtSecurityToken(
            claims: updatedClaims,
            notBefore: GetJwtValidBeforeTime(dateTime),
            expires: GetUserJwtExpirationTime(dateTime, securityConfig),
            signingCredentials: GetSigningCredentials(securityConfig),
            issuer: GetJwtIssuer(appConfig),
            audience: GetJwtAudience(appConfig));

        return JwtHandler.WriteToken(token);
    }

    public static string GenerateApiJwtEncryptedToken(IEnumerable<Claim> claims, IDateTimeService dateTime, SecurityConfiguration securityConfig,
        AppConfiguration appConfig)
    {
        var updatedClaims = claims.ToList();
        updatedClaims.Add(new Claim(ClaimConstants.AuthenticationType, ClaimConstants.AuthType.Api));

        var token = new JwtSecurityToken(
            claims: updatedClaims,
            notBefore: GetJwtValidBeforeTime(dateTime),
            expires: GetApiJwtExpirationTime(dateTime, securityConfig),
            signingCredentials: GetSigningCredentials(securityConfig),
            issuer: GetJwtIssuer(appConfig),
            audience: GetJwtAudience(appConfig));

        return JwtHandler.WriteToken(token);
    }

    public static string GenerateHostJwtEncryptedToken(IEnumerable<Claim> claims, IDateTimeService dateTime, SecurityConfiguration securityConfig,
        AppConfiguration appConfig)
    {
        var updatedClaims = claims.ToList();
        updatedClaims.Add(new Claim(ClaimConstants.AuthenticationType, ClaimConstants.AuthType.Host));

        var token = new JwtSecurityToken(
            claims: updatedClaims,
            notBefore: GetJwtValidBeforeTime(dateTime),
            expires: GetApiJwtExpirationTime(dateTime, securityConfig),
            signingCredentials: GetSigningCredentials(securityConfig),
            issuer: GetJwtIssuer(appConfig),
            audience: GetJwtAudience(appConfig));

        return JwtHandler.WriteToken(token);
    }

    public static string GenerateUserJwtRefreshToken(IDateTimeService dateTime, SecurityConfiguration securityConfig, AppConfiguration appConfig,
        Guid userId)
    {
        // Refresh token should only have the ID as to not allow someone to access to anything, extra layer of abstraction
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: GetJwtValidBeforeTime(dateTime),
            expires: GetJwtRefreshTokenExpirationTime(dateTime, securityConfig),
            signingCredentials: GetSigningCredentials(securityConfig),
            issuer: GetJwtIssuer(appConfig),
            audience: GetJwtAudience(appConfig));

        return JwtHandler.WriteToken(token);
    }

    public static TokenValidationParameters GetJwtValidationParameters(SecurityConfiguration securityConfig, AppConfiguration appConfig)
    {
        return GetJwtValidationParameters(GetJwtSecret(securityConfig), GetJwtIssuer(appConfig), GetJwtAudience(appConfig));
    }

    public static ClaimsPrincipal? GetClaimsPrincipalFromToken(string? token, SecurityConfiguration securityConfig, AppConfiguration appConfig)
    {
        try
        {
            var validator = GetJwtValidationParameters(securityConfig, appConfig);

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var claimsPrincipal = JwtHandler.ValidateToken(token, validator, out _);
            return claimsPrincipal;
        }
        catch (SecurityTokenExpiredException)
        {
            // User principal has expired / token has expired so we'll return an expired principal
            return UserConstants.ExpiredPrincipal;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool IsJwtValid(string? token, SecurityConfiguration securityConfig, AppConfiguration appConfig)
    {
        var claimsPrincipal = GetClaimsPrincipalFromToken(token, securityConfig, appConfig);

        if (claimsPrincipal is null)
            return false;
        if (claimsPrincipal == UserConstants.UnauthenticatedPrincipal)
            return false;
        if (claimsPrincipal == UserConstants.ExpiredPrincipal)
            return false;

        return true;
    }
}