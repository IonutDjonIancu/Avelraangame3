using System.ComponentModel.DataAnnotations;

namespace Models;

public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    [MaxLength(10)]
    public List<Character> Characters { get; set; } = [];

    [MaxLength(1000)]
    public List<Character> Graveyard { get; set; } = [];
}
