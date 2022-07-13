namespace Tgs.PlayerService.ApplicationServices;
public record RecordLoginIpCommand(
    string PlayerCode,
    string Ip,
    string? HostName);
