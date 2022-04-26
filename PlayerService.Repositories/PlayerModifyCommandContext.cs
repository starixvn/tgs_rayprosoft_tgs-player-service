using Tgs.PlayerService.Repositories.Entities;

namespace Tgs.PlayerService.Repositories;
internal record struct PlayerModifyCommandContext(
    Player Player,
    Dictionary<string, PlayerProperty> PlayerProperties,
    TgsContext TgsContext) : IDataCommandContext;
