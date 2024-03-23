using Models;

namespace Avelraangame3.Persistence;

public class Snapshot
{
    public List<Character> Characters { get; set; } = [];

    public Character Character { get; set; } = new();
}
