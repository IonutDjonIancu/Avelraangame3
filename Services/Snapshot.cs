using Models;

namespace Services;

public interface ISnapshot
{
    HashSet<Character> Characters { get; set; }

    List<Board> Boards { get; set; }

    HashSet<Guid> ItemsSold { get; set; }

    List<Item> Market {  get; set; }    
}

public class Snapshot : ISnapshot
{
    public HashSet<Character> Characters { get; set; } = [];

    public List<Board> Boards { get; set; } = [];

    public HashSet<Guid> ItemsSold { get; set; } = [];

    public List<Item> Market { get; set; } = [];
}
