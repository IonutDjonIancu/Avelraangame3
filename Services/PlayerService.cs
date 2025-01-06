using Models;

namespace Services;

public interface IPlayerService
{
    Player CreatePlayer(string name);
}

public class PlayerService : IPlayerService
{
    private readonly ISnapshot _snapshot;
    private readonly IValidatorService _validatorService;

    public PlayerService(
        ISnapshot snapshot,
        IValidatorService validatorService)
    {
        _validatorService = validatorService;
        _snapshot = snapshot;
    }

    public Player CreatePlayer(string name)
    {
        _validatorService.ValidateOnCreatePlayer(name);

        var player = new Player
        {
            Name = name,
            Id = Guid.NewGuid()
        };

        _snapshot.Players.Add(player);

        return player;
    }



}
