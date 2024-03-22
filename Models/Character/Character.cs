namespace Models;

public class Character
{
    public CharacterIdentity Identity { get; set; } = new();
    public CharacterTraits Traits { get; set; } = new();
    public CharacterStats Stats { get; set; } = new();
    public CharacterAssets Assets { get; set; } = new();
    public CharacterSkills Skills { get; set; } = new();
}
