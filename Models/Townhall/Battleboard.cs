namespace Models;

public class Battleboard
{
    public Guid Id { get; set; }
    public string Type { get; set; }

    public List<Character> GoodGuys { get; set; } = [];
    public List<Character> BadGuys { get; set; } = [];

    public string Result { get; set; }
}

public class Duel : Battleboard
{
    public List<Guid> Battlequeue { get; set; } = [];
    public int RoundNr { get; set; }
}

