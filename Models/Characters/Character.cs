using System.ComponentModel.DataAnnotations;

namespace Models;

public class Character
{
    public CharacterIdentity Identity { get; set; } = new();
    public CharacterDetails Details { get; set; } = new();
    public Stats Stats { get; set; } = new();

    [MaxLength(4)]
    public List<Item> Inventory { get; set; } = [];
    [MaxLength(10)]
    public List<Trinket> Regalia { get; set; } = [];

    public CharacterSupplies Supplies { get; set; } = new();

    public List<string> SpecialSkills { get; set; } = [];
}

public class Stats
{
    public CharacterStats Base { get; set; } = new();
    public CharacterStats Actual { get; set; } = new();
    public CharacterStats Fight { get; set; } = new();
}

public class Characters
{
    public List<CharacterVm> CharactersList { get; set; } = [];
}

public class CharacterVm
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Portrait { get; set; }
    public int Wealth { get; set; }
    public bool IsLocked { get; set; }
}

public class CharacterIdentity
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
}

public class CharacterIdentityVm
{
    public Guid Id { get; set; }
}

public class CharacterDetails
{
    public string Name { get; set; }
    public string Race { get; set; }
    public string Culture { get; set; }
    public string Spec { get; set; }
    public string Portrait { get; set; }
    public int Entitylevel { get; set; }
    public int Levelup { get; set; }
    public int Wealth { get; set; }
    public int Worth { get; set; }
    public bool IsHidden { get; set; }
    public bool IsAlive { get; set; }
    public bool IsLocked { get; set; }
    public bool IsNpc { get; set; }

    public Guid BoardId { get; set; }
    public string BoardType { get; set; }

    /// <summary>
    /// Used to count how many core rules fights the character has survived.
    /// </summary>
    public int Renown { get; set; } 
}

public class CharacterStats
{
    // main
    public int Strength { get; set; }
    public int Constitution { get; set; }
    public int Agility { get; set; }
    public int Willpower { get; set; }
    public int Abstract { get; set; }
    // skills
    public int Melee { get; set; }
    public int Arcane { get; set; }
    public int Psionics { get; set; }
    public int Social { get; set; }
    public int Hide { get; set; }
    public int Survival { get; set; }
    public int Tactics { get; set; }
    public int Aid { get; set; }
    public int Crafting { get; set; }
    public int Perception { get; set; }
    // assets
    public int Defense { get; set; }
    public int Actions { get; set; }
    public int Hitpoints { get; set; }
    public int Mana { get; set; }
}

public class CharacterSupplies
{
    public List<Item> Items { get; set; } = [];
    public List<Trinket> Regalia { get; set; } = [];
}

