namespace Tgs.PlayerService.ViewModels;

public record PlayerDetailViewModel(
    string PlayerCode,
    IReadOnlyDictionary<string, string?> Properties);
