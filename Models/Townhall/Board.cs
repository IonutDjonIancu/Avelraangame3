namespace Models;

public class Board
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Type { get; set; }
    public string EffortLevelName { get; set; }

    // party leader is always player character starting from index 0
    public List<CharacterVm> GoodGuys { get; set; } = [];
    public List<CharacterVm> BadGuys { get; set; } = [];

    public string Message { get; set; }

    public Rewards Rewards { get; set; } = new();

    public List<CharacterVm> Battlequeue { get; set; } = [];

    public int RoundNr { get; set; }
}

public class Duel : Board
{
}

public class Rewards
{
    public int Wealth { get; set; }
    public List<Item> Items { get; set; } = [];
}
