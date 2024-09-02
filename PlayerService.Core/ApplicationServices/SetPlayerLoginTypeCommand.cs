namespace Tgs.PlayerService.ApplicationServices;
public record SetPlayerLoginTypeCommand(
    string PlayerCode,
    PlayerLoginType LoginType,
    string? Modifier);