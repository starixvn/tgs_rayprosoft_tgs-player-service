namespace Tgs.PlayerService.Core;

public record PlayerDetailInfo(
    string PlayerCode,
    string? RealName,
    decimal DepositedAmount,
    string? AreaCode,
    string? Phone,
    string? Email,
    IReadOnlyDictionary<string, string> Properties);
