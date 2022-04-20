namespace Tgs.PlayerService.Client;

public class PlayerServiceApiException : Exception
{
    public PlayerServiceApiException(string? message)
        : base(message)
    {
    }
}
