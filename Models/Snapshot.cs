namespace Models;

public interface ISnapshot
{
    HashSet<Player> Players { get; set; }
    HashSet<Character> Npcs { get; set; }
    
    List<Board> Boards { get; set; }

    HashSet<Item> Market {  get; set; }    
}

public class Snapshot : ISnapshot
{
    public HashSet<Player> Players { get; set; } = [];
    public HashSet<Character> Npcs { get; set; } = [];

    public List<Board> Boards { get; set; } = [];

    public HashSet<Item> Market { get; set; } = [];
}
