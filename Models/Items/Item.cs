namespace Models;

public class Trinket : Item
{
    public bool IsPermanent { get; set; }
}

public class Item
{
    public bool HasTaint { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int Level { get; set; }

    public CharacterStats Stats { get; set; } = new();
    public CharacterAssets Assets { get; set; } = new();
    public CharacterCrafts Crafts { get; set; } = new();
    public CharacterAbilities Abilities { get; set; } = new();

    public int Value { get; set; }
}