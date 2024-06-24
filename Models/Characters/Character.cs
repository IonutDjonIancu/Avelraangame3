using System.ComponentModel.DataAnnotations;

namespace Models;

public class CharacterBase
{
    public CharacterDetails Details { get; set; } = new();
    public CharacterStats Stats { get; set; } = new();
    public CharacterFeats Feats { get; set; } = new();
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

public class CharacterNpc : CharacterBase
{
    public CharacterIdentity Identity { get; set; } = new();
}

public class CharactersVm
{
    public List<string> CharactersPortraits { get; set; }
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
    public CharacterFeats Feats { get; set; } = new();
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
    public Guid BattleboardId { get; set; }
    public string BattleboardType { get; set; }
}

public class CharacterStats
{
    public int Defense { get; set; }
    public int Resist { get; set; }
    public int Actions { get; set; }
    public int Endurance { get; set; }
    public int Accretion { get; set; }
}

public class CharacterFeats
{
    public int Combat { get; set; }
    public int Strength { get; set; }
    public int Tactics { get; set; }
    public int Athletics { get; set; }
    public int Survival { get; set; }
    public int Social { get; set; }
    public int Abstract { get; set; }
    public int Psionic { get; set; }
    public int Crafting { get; set; }
    public int Medicine { get; set; }

    public int CombatEff { get; set; }
    public int StrengthEff { get; set; }
    public int TacticsEff { get; set; }
    public int AthleticsEff { get; set; }
    public int SurvivalEff { get; set; }
    public int SocialEff { get; set; }
    public int AbstractEff { get; set; }
    public int PsionicEff { get; set; }
    public int CraftingEff { get; set; }
    public int MedicineEff { get; set; }
}

public class CharacterSupplies
{
    public List<Item> Items { get; set; } = [];
    public List<Trinket> Regalia { get; set; } = [];
}

