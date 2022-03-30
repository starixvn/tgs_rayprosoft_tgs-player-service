namespace Tgs.PlayerService.ViewModels;

public record PlayerDetailViewModel(
    string PlayerCode,
    string? RealName,
    decimal DepositedAmount,
    string? AreaCode,
    string? Phone,
    string? Email,
    IReadOnlyDictionary<string, string> Properties);
