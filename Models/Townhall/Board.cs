namespace Models;

public class Board
{
    public Guid Id { get; set; }
    public string Type { get; set; }

    // party leader is the first who joins
    public List<Character> GoodGuys { get; set; } = [];
    public List<Character> BadGuys { get; set; } = [];

    public string Message { get; set; }
}

public class Duel : Board
{
    public List<Guid> Battlequeue { get; set; } = [];
    public int RoundNr { get; set; }
}

