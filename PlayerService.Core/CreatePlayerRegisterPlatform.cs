namespace Tgs.PlayerService;

public record CreatePlayerRegisterPlatform
{
    public string PlayerCode { get; init; } = default!;

    public RegisterPlatformType RegisterPlatformType { get; init; }

    public string CreatedByUsername { get; init; } = default!;
}