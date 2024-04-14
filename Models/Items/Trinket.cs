namespace Models;

public class Trinket
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public int Level { get; set; }
    public bool IsPermanent { get; set; }

    public CharacterAssets Assets { get; set; } = new();

    public int Value { get; set; }
}
