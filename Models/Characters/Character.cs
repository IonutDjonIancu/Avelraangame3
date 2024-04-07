using System.ComponentModel.DataAnnotations;

namespace Models;

public class Character
{
    public CharacterIdentity Identity { get; set; } = new();
    public CharacterTraits Traits { get; set; } = new();
    public CharacterDetails Details { get; set; } = new();
    public CharacterStats Stats { get; set; } = new();
    public CharacterAssets Assets { get; set; } = new();
    public CharacterCrafts Crafts { get; set; } = new();

    [MaxLength(4)]
    public List<Item> Items { get; set; } = [];
}
