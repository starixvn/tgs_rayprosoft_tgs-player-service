namespace Tgs.PlayerService.ViewModels;

public class PlayerCodesQueryViewModel : IParsable<PlayerCodesQueryViewModel>
{
    public IEnumerable<string> UserNames { get; init; } = Array.Empty<string>();

    public static PlayerCodesQueryViewModel Parse(string s, IFormatProvider? provider)
    {
        return !TryParse(s, provider, out var result) 
            ? throw new ArgumentException("Could not parse supplied value.", nameof(s)) 
            : result;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out PlayerCodesQueryViewModel result)
    {
        if (s is null)
        {
            result = new PlayerCodesQueryViewModel
            {
                UserNames = Array.Empty<string>()
            };

            return false;
        }

        var userNames = s.Split(',', StringSplitOptions.RemoveEmptyEntries
                                         | StringSplitOptions.TrimEntries);
        
        result = new PlayerCodesQueryViewModel
        {
            UserNames = userNames
        };

        return true;
    }
}