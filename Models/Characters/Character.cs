using System.ComponentModel.DataAnnotations;

namespace Models;

public class CharacterBase
{
    public CharacterDetails Details { get; set; } = new();
    public CharacterStats Stats { get; set; } = new();
    public CharacterCrafts Crafts { get; set; } = new();
    public CharacterActuals Actuals { get; set; } = new();

    [MaxLength(4)]
    public List<Item> Inventory { get; set; } = [];
    [MaxLength(10)]
    public List<Trinket> Regalia { get; set; } = [];

    public CharacterSupplies Supplies { get; set; } = new();

    // TODO: add special skills
}

public class Character : CharacterBase
{
    public CharacterIdentity Identity { get; set; } = new();
}

public class CharacterVm : CharacterBase
{
    public CharacterIdentityVm Identity { get; set; } = new();
}

public class CharacterIdentity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
}

public class CharacterIdentityVm
{
    public Guid Id { get; set; }
}

public class CharacterActuals
{
    public CharacterStats Stats { get; set; } = new();
    public CharacterCrafts Crafts { get; set; } = new();
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
    public bool IsHidden { get; set; }
    public bool IsAlive { get; set; }
    public bool IsLocked { get; set; }
    public bool IsNpc { get; set; }
}

public class CharacterStats
{
    public int Strength { get; set; }
    public int Athletics { get; set; }
    public int Willpower { get; set; }
    public int Abstract { get; set; }
    public int Harm { get; set; }
    public int Fortitude { get; set; }
    public int Accretion { get; set; }
    public int Guile { get; set; }
    public int Awareness { get; set; }
    public int Charm { get; set; }
    public int Apcom { get; set; }
    public int Defense { get; set; }
    public int Resist { get; set; }
    public int Hitpoints { get; set; }
    public int Mana { get; set; }
}

public class CharacterCrafts
{
    public int Combat { get; set; } // dodge included
    public int Arcane { get; set; }
    public int Alchemy { get; set; } // potions and wearables
    public int Psionics { get; set; }
    public int Hunting { get; set; } // hide included
    public int Advocacy { get; set; }
    public int Mercantile { get; set; }
    public int Tactics { get; set; }
    public int Travelling { get; set; }
    public int Medicine { get; set; }
    public int Sailing { get; set; }
}

public class CharacterSupplies
{
    public List<Item> Items { get; set; } = [];
    public List<Trinket> Trinkets { get; set; } = [];
}

