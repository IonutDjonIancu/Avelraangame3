using Models;

namespace Services;

public interface ISnapshot
{
    HashSet<Character> Characters { get; set; }
}

public class Snapshot : ISnapshot
{
    public HashSet<Character> Characters { get; set; } = [];
}
