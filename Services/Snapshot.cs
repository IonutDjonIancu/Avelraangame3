using Models;

namespace Services;

public interface ISnapshot
{
    HashSet<Character> Characters { get; set; }

    List<Battleboard> Battleboards { get; set; }

    HashSet<Guid> ItemsSold { get; set; }
}

public class Snapshot : ISnapshot
{
    public HashSet<Character> Characters { get; set; } = [];

    public List<Battleboard> Battleboards { get; set; } = [];

    public HashSet<Guid> ItemsSold { get; set; } = [];
}
