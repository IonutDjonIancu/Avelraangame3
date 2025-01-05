namespace Models;

public interface ISnapshot
{
    HashSet<Player> Players { get; set; }

    List<Board> Boards { get; set; }

    HashSet<Guid> ItemsSold { get; set; }

    List<Item> Market {  get; set; }    
}

public class Snapshot : ISnapshot
{
    public HashSet<Player> Players { get; set; } = [];

    public List<Board> Boards { get; set; } = [];

    public HashSet<Guid> ItemsSold { get; set; } = [];

    public List<Item> Market { get; set; } = [];
}
