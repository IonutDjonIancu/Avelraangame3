namespace Models;

public class Board
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string EffortLevelName { get; set; }

    // party leader is the first who joins
    public List<Character> GoodGuys { get; set; } = [];
    public List<Character> BadGuys { get; set; } = [];

    public string Message { get; set; }

    public Rewards Rewards { get; set; } = new();
}

public class Duel : Board
{
    public List<Guid> Battlequeue { get; set; } = [];
    public int RoundNr { get; set; }
}

public class Rewards
{
    public int Wealth { get; set; }
    public List<Item> Items { get; set; } = [];
}
