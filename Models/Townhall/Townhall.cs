namespace Models;

public class MarketForPlayer
{
    public Items Items { get; set; } = new();

    public Characters Characters { get; set; } = new();
}

public class DuelForPlayer
{
    public Characters Characters { get; set; } = new();
}