namespace Tgs.PlayerService;

public record CreatePlayerRegisterPlatformTag
{
    public string PlayerCode { get; init; } = default!;

    public RegisterPlatformType RegisterPlatformType { get; init; }
}