namespace Tgs.PlayerService.Repositories;

internal class PlayerDataCommandBuilder : IDataCommandBuilder
{
    public IDataCommand CreateDeleteCommand(string name)
        => new PlayerDeleteDataCommand(name);

    public IDataCommand CreatePutCommand(string name, string value)
        => new PlayerPutDataCommand(name, value);
}
