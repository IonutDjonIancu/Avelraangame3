namespace Models;

public class Item
{
    public Guid Id { get; set; }

    public int Level { get; set; }
    public bool HasTaint { get; set; }

    public string Type { get; set; }
    public string Name { get; set; }

    public CharacterCrafts Crafts { get; set; } = new();
    public CharacterStats Stats { get; set; } = new();
    public CharacterAssets Assets { get; set; } = new();

    public int Value { get; set; }
}


