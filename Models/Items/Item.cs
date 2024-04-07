namespace Models;

public class Item
{
    public Guid Id { get; set; }

    public int Level { get; set; }
    public bool HasTaint { get; set; }

    public Statics.ItemType Type { get; set; }
    public string Name { get; set; }

    public CharacterCrafts Crafts { get; set; }
    public CharacterStats Stats { get; set; }
    public CharacterAssets Assets { get; set; }

    public int Value { get; set; }
}


