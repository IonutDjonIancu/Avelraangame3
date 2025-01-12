using System.ComponentModel.DataAnnotations;

namespace Models;

public interface ISnapshot
{
    [MaxLength(50)]
    List<Player> Players { get; set; }

    List<Character> Characters { get; set; }

    List<Board> Boards { get; set; }

    List<Item> MarketItems {  get; set; }
}

public class Snapshot : ISnapshot
{
    [MaxLength(50)]
    public List<Player> Players { get; set; } = [];

    public List<Character> Characters { get; set; } = [];

    public List<Board> Boards { get; set; } = [];

    [MaxLength (10000)]
    public List<Item> MarketItems { get; set; } = [];
}
