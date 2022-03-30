namespace Tgs.PlayerService.Core;

public record PlayerDetailInfo(
    string PlayerCode,
    IReadOnlyDictionary<string, string?> Properties);
