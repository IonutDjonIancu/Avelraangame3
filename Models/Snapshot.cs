using System.ComponentModel.DataAnnotations;

namespace Models;

public interface ISnapshot
{
    List<Player> Players { get; set; }
    List<Character> Npcs { get; set; }
    
    List<Board> Boards { get; set; }

    List<Item> MarketItems {  get; set; }

    List<Character> GetAllCharacters();
}

public class Snapshot : ISnapshot
{
    [MaxLength(100)]
    public List<Player> Players { get; set; } = [];

    [MaxLength(100)]
    public List<Character> Npcs { get; set; } = [];

    [MaxLength(100)]
    public List<Board> Boards { get; set; } = [];

    [MaxLength (10000)]
    public List<Item> MarketItems { get; set; } = [];

    public List<Character> GetAllCharacters()
    {
        return Players.SelectMany(p => p.Characters).ToList();
    }
}
