using Domain.Enums.Integrations;

namespace Application.Helpers.Integrations;

public static class ExternalAuthHelpers
{
    private static string RedirectToString(ExternalAuthRedirect redirect)
    {
        return redirect switch
        {
            ExternalAuthRedirect.Login => "redindxlin",
            ExternalAuthRedirect.Register => "redstr",
            ExternalAuthRedirect.Security => "redsrty",
            _ => "redindxlin"
        };
    }

    private static ExternalAuthRedirect StringToRedirect(string redirect)
    {
        return redirect switch
        {
            "redindxlin" => ExternalAuthRedirect.Login,
            "redstr" => ExternalAuthRedirect.Register,
            "redsrty" => ExternalAuthRedirect.Security,
            _ => ExternalAuthRedirect.Login
        };
    }

    public static ExternalAuthProvider StringToProvider(string provider)
    {
        var isValidProvider = Enum.TryParse(provider, out ExternalAuthProvider parsedProvider);
        return !isValidProvider ? ExternalAuthProvider.Unknown : parsedProvider;
    }
    
    public static string GetAuthRedirectState(ExternalAuthProvider provider, ExternalAuthRedirect redirect)
    {
        return string.Join("-", provider.ToString(), RedirectToString(redirect));
    }

    public static (ExternalAuthProvider provider, ExternalAuthRedirect redirect) GetStateFromRedirect(string state)
    {
        try
        {
            var stateSplit = state.Split('-');
            var provider = StringToProvider(stateSplit[0]);
            var redirect = StringToRedirect(stateSplit[1]);
            return new ValueTuple<ExternalAuthProvider, ExternalAuthRedirect>(provider, redirect);
        }
        catch (Exception)
        {
            return new ValueTuple<ExternalAuthProvider, ExternalAuthRedirect>(ExternalAuthProvider.Google, ExternalAuthRedirect.Login);
        }
    }
}