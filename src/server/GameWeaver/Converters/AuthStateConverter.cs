using Domain.Enums.Identity;

namespace GameWeaver.Converters;

public class AuthStateConverter : BoolConverter<AuthState>
{
    public AuthStateConverter()
    {
        SetFunc = OnSet;
        GetFunc = OnGet;
    }
    
    private AuthState OnGet(bool? value) => value == true ? AuthState.Enabled : AuthState.Disabled;

    private bool? OnSet(AuthState state)
    {
        return state != AuthState.Disabled;
    }
}