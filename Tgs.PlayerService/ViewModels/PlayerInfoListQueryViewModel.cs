namespace Tgs.PlayerService.ViewModels;

public class PlayerInfoListQueryViewModel : IParsable<PlayerInfoListQueryViewModel>
{
    public IEnumerable<string> PlayerCodes { get; init; } = Array.Empty<string>();

    public static PlayerInfoListQueryViewModel Parse(string s, IFormatProvider? provider)
        => !TryParse(s, provider, out var result)
            ? throw new ArgumentException("Could not parse supplied value.", nameof(s))
            : result;

    public static bool TryParse(string? s, IFormatProvider? provider, out PlayerInfoListQueryViewModel result)
    {
        if (s is null)
        {
            result = new PlayerInfoListQueryViewModel
            {
                PlayerCodes = Array.Empty<string>()
            };

            return false;
        }

        var playerCodes = s.Split(',', StringSplitOptions.RemoveEmptyEntries
                                     | StringSplitOptions.TrimEntries);

        result = new PlayerInfoListQueryViewModel
        {
            PlayerCodes = playerCodes
        };

        return true;
    }
}