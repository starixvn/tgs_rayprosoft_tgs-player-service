namespace Tgs.PlayerService;

public record CreatePlayerData
{
    public Guid? AgentId { get; init; }

    public int BetLimitId { get; init; }

    public Guid BrandId { get; init; }

    public Guid VipId { get; init; }

    public string CurrencyCode { get; init; } = default!;

    public byte[] Password { get; init; } = Array.Empty<byte>();

    public PlayerStatus PlayerStatus { get; init; }

    public byte[] Salt { get; init; } = Array.Empty<byte>();

    public PlayerType Type { get; init; }

    public string? UpperPlayerCode { get; init; }

    public string UserName { get; init; } = default!;

    public byte[] WithdrawPassword { get; init; } = Array.Empty<byte>();

    public byte[] WithdrawSalt { get; init; } = Array.Empty<byte>();
}