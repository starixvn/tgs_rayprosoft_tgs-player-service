namespace Tgs.PlayerService.ApplicationServices;

public record SetPlayerPasswordCommand(
    string PlayerCode,
    string Password,
    string? ModifyUsername,
    string? ModifyUserId);
