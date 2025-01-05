namespace Models;

public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public List<Character> Characters { get; set; }

    public HashSet<Character> Graveyard { get; set; }
}
