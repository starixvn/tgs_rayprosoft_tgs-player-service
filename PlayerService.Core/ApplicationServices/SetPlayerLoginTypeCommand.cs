namespace Tgs.PlayerService.ApplicationServices;
public record SetPlayerLoginTypeCommand(
    string PlayerCode,
    string LoginType,
    string? Modifier);