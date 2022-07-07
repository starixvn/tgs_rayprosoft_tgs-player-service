namespace Tgs.PlayerService.ApplicationServices;
public record RecordRegisterIpCommand(
    string PlayerCode,
    string Ip,
    string? HostName);
