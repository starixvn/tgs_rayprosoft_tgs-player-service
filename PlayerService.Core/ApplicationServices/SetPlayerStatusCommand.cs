namespace Tgs.PlayerService.ApplicationServices;
public record SetPlayerStatusCommand(string PlayerCode, PlayerStatus Status);
