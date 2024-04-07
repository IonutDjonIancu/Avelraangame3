using Models;

namespace Services;

public interface ISnapshot
{
    List<Character> Characters { get; set; }
}

public class Snapshot : ISnapshot
{
    public List<Character> Characters { get; set; } = [];
}
