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
    public List<Item> Inventory { get; set; } = [];
    public List<Item> Supplies { get; set; } = [];

    [MaxLength(10)]
    public List<Trinket> Trinkets { get; set;} = [];

    // TODO: add special skills
}

public class CharacterIdentity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
}

public class CharacterTraits
{
    public string Race { get; set; }
    public string Culture { get; set; }
    public string Spec { get; set; }
}

public class CharacterDetails
{
    public string Name { get; set; }
    public string Portrait { get; set; }
    public int Entitylevel { get; set; }
    public int Levelup { get; set; }
    public bool IsHidden { get; set; }
    public bool IsAlive { get; set; }
    public bool IsLocked { get; set; }
    public bool IsNpc { get; set; }

    public int Wealth { get; set; }
}

public class CharacterStats
{
    public int Strength { get; set; }
    public int Athletics { get; set; }
    public int Willpower { get; set; }
    public int Abstract { get; set; }
}


public class CharacterAssets
{
    public int HitpointsBase { get; set; }
    public int HitpointsActual { get; set; }

    public int ManaBase { get; set; }
    public int ManaActual { get; set; }

    public int ActionsBase { get; set; }
    public int ActionsActual { get; set; }

    public int DefenseBase { get; set; }
    public int DefenseActual { get; set; }

    public int ResistBase { get; set; }
    public int ResistActual { get; set; }

    public int ReflexBase { get; set; }
    public int ReflexActual { get; set; }
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
    public int Sailing { get; set; }
    public int Medicine { get; set; }
}
