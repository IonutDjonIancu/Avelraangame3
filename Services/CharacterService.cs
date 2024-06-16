using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Services;

public interface ICharacterService
{
    CharacterVm GetCharacter(Guid id, Guid sessionId);
    string CreateCharacter(CreateCharacter createCharacter);
    ImportCharacterResponse ImportCharacter(ImportCharacter characterString);

    CharacterVm EquipItem(EquipItem equipItem);
    CharacterVm UnequipItem(EquipItem equipItem);
    CharacterVm SellItem(EquipItem equipItem);
    CharacterVm Levelup(CharacterLevelup levelup);
}

public class CharacterService : ICharacterService
{
    private readonly ISnapshot ss;
    private readonly IItemService _items;
    private readonly IDiceService _dice;

    public CharacterService(
        ISnapshot snapshot,
        IItemService itemService,
        IDiceService dice)
    {
        ss = snapshot;
        _items = itemService;
        _dice = dice;
    }

    public CharacterVm Levelup(CharacterLevelup levelup)
    {
        var character = Validators.ValidateLevelupAndReturn(levelup, ss);

        int value;

        if (Statics.Stats.All.Contains(levelup.Attribute))
        {
            var stat = typeof(CharacterStats).GetProperty(levelup.Attribute)!;
            value = (int)stat.GetValue(character.Stats)!;
            stat.SetValue(character.Stats, value + 1);
        }
        else
        {
            var craft = typeof(CharacterCrafts).GetProperty(levelup.Attribute)!;
            value = (int)craft.GetValue(character.Crafts)!;
            craft.SetValue(character.Crafts, value + 1);
        }

        if (value <= 0)
        {
            character.Details.Levelup -= 1;
        }
        else
        {
            character.Details.Levelup -= value * 2;
        }

        return GetCharacter(character.Identity);
    }

    public CharacterVm SellItem(EquipItem equipItem)
    {
        var (item, character) = Validators.ValidateSellItemAndReturn(equipItem, ss)!;

        var charVm = GetCharacter(character.Identity);

        var roll = _dice.Roll_vs_effort(charVm, Statics.Crafts.Mercantile, Statics.EffortLevels.Easy);

        if (item.Type == Statics.Items.Types.Trinket)
        {
            character.Supplies.Regalia.Remove(item as Trinket);
        }
        else
        {
            character.Supplies.Items.Remove(item);
        }

        character.Details.Wealth += (int)(item.Value * roll);

        return GetCharacter(character.Identity);
    }

    public CharacterVm UnequipItem(EquipItem equipItem)
    {
        var (item, character) = Validators.ValidateUnequipItemAndReturn(equipItem, ss)!;

        if (item.Type == Statics.Items.Types.Trinket)
        {
            character.Supplies.Regalia.Add(item as Trinket);
            character.Regalia.Remove(item as Trinket);
        } 
        else
        {
            character.Supplies.Items.Add(item);
            character.Inventory.Remove(item);
        }

        return GetCharacter(character.Identity);
    }

    public CharacterVm EquipItem(EquipItem equipItem)
    {
        var (item, character) = Validators.ValidateEquipItemAndReturn(equipItem, ss)!;

        if (item.Type == Statics.Items.Types.Trinket)
        {
            character.Regalia.Add(item as Trinket);
            character.Supplies.Regalia.Remove(item as Trinket);
        }
        else
        {
            character.Inventory.Add(item);
            character.Supplies.Items.Remove(item);
        }

        return GetCharacter(character.Identity);
    }

    public ImportCharacterResponse ImportCharacter(ImportCharacter import)
    {
        Validators.ValidateOnImportCharacter(import);
        var decryptString = EncryptionService.DecryptString(import.CharacterString);

        var character = JsonConvert.DeserializeObject<Character>(decryptString)!;

        var oldChar = ss.Characters.FirstOrDefault(s => s.Identity.Id == character.Identity.Id);

        if (oldChar != null)
            ss.Characters.Remove(oldChar);
        
        ss.Characters.Add(character);

        return new ImportCharacterResponse
        {
            CharacterId = character.Identity.Id,
            SessionId = character.Identity.SessionId
        };
    }

    public string CreateCharacter(CreateCharacter create)
    {
        Validators.ValidateOnCreateCharacter(create);

        var character = new Character
        {
            Identity = new CharacterIdentity
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
            },
        };

        SetDetails(create, character);
        SetStats(create, character);
        SetCrafts(create, character);
        SetInventory(character);
        SetTrinkets(character);


        // *********************************************************************** TODO: remove this part ***********************************************************************
        character.Supplies.Items.AddRange(_items.GenerateRandomItems(5));
        character.Details.Levelup = 10000;
        // *********************************************************************** TODO: remove this part ***********************************************************************

        ss.Characters.Add(character);

        var characterEncr = EncryptionService.EncryptString(JsonConvert.SerializeObject(character));

        return characterEncr;
    }

    public CharacterVm GetCharacter(CharacterIdentity identity)
    {
        return GetCharacter(identity.Id, identity.SessionId);
    }

    public CharacterVm GetCharacter(Guid id, Guid sessionId)
    {
        Validators.ValidateOnGetCharacter(id, sessionId);

        var character = ss.Characters.FirstOrDefault(s => s.Identity.Id == id && s.Identity.SessionId == sessionId);

        Validators.ValidateAgainstNull(character!, "No character found. Go to import first.");

        var charVm =  new CharacterVm
        {
            Identity = new CharacterIdentityVm
            {
                Id = id,
            },
            Details = character!.Details,
            Stats = character.Stats,
            Crafts = character.Crafts,
            Inventory = character.Inventory,
            Regalia = character.Regalia,
            Supplies = character.Supplies
        };

        CalculateActuals(charVm);

        if (charVm.Actuals.Stats.Defense > 90)
        {
            charVm.Actuals.Stats.Defense = 90; // capped at 90 %
        }

        if (charVm.Actuals.Stats.Resist > 100)
        {
            charVm.Actuals.Stats.Resist = 100; // capped at 100 %
        }
        
        return charVm;
    }

    #region private methods
    private static void CalculateActuals(CharacterBase character)
    {
        character.Actuals.Stats.Strength    += character.Stats.Strength     + character.Inventory.Select(s => s.Stats.Strength).Sum()   + character.Regalia.Select(s => s.Stats.Strength).Sum();
        character.Actuals.Stats.Athletics   += character.Stats.Athletics    + character.Inventory.Select(s => s.Stats.Athletics).Sum()  + character.Regalia.Select(s => s.Stats.Athletics).Sum();
        character.Actuals.Stats.Willpower   += character.Stats.Willpower    + character.Inventory.Select(s => s.Stats.Willpower).Sum()  + character.Regalia.Select(s => s.Stats.Willpower).Sum();
        character.Actuals.Stats.Abstract += character.Stats.Abstract + character.Inventory.Select(s => s.Stats.Abstract).Sum() + character.Regalia.Select(s => s.Stats.Abstract).Sum();
        character.Actuals.Stats.Harm += character.Stats.Harm + character.Inventory.Select(s => s.Stats.Harm).Sum() + character.Regalia.Select(s => s.Stats.Harm).Sum();
        character.Actuals.Stats.Fortitude += character.Stats.Fortitude + character.Inventory.Select(s => s.Stats.Fortitude).Sum() + character.Regalia.Select(s => s.Stats.Fortitude).Sum();
        character.Actuals.Stats.Accretion += character.Stats.Accretion + character.Inventory.Select(s => s.Stats.Accretion).Sum() + character.Regalia.Select(s => s.Stats.Accretion).Sum();
        character.Actuals.Stats.Guile += character.Stats.Guile + character.Inventory.Select(s => s.Stats.Guile).Sum() + character.Regalia.Select(s => s.Stats.Guile).Sum();
        character.Actuals.Stats.Awareness += character.Stats.Awareness + character.Inventory.Select(s => s.Stats.Awareness).Sum() + character.Regalia.Select(s => s.Stats.Awareness).Sum();
        character.Actuals.Stats.Charm += character.Stats.Charm + character.Inventory.Select(s => s.Stats.Charm).Sum() + character.Regalia.Select(s => s.Stats.Charm).Sum();
        character.Actuals.Stats.Defense += character.Stats.Defense + character.Inventory.Select(s => s.Stats.Defense).Sum() + character.Regalia.Select(s => s.Stats.Defense).Sum();
        character.Actuals.Stats.Resist += character.Stats.Resist + character.Inventory.Select(s => s.Stats.Resist).Sum() + character.Regalia.Select(s => s.Stats.Resist).Sum();
        character.Actuals.Stats.Apcom += character.Stats.Apcom + character.Inventory.Select(s => s.Stats.Apcom).Sum() + character.Regalia.Select(s => s.Stats.Apcom).Sum();
        character.Actuals.Stats.Hitpoints += character.Stats.Hitpoints + character.Inventory.Select(s => s.Stats.Hitpoints).Sum() + character.Regalia.Select(s => s.Stats.Hitpoints).Sum();
        character.Actuals.Stats.Mana += character.Stats.Mana + character.Inventory.Select(s => s.Stats.Mana).Sum() + character.Regalia.Select(s => s.Stats.Mana).Sum();

        character.Actuals.Crafts.Combat += character.Crafts.Combat + character.Inventory.Select(s => s.Crafts.Combat).Sum() + character.Regalia.Select(s => s.Crafts.Combat).Sum();
        character.Actuals.Crafts.Arcane += character.Crafts.Arcane + character.Inventory.Select(s => s.Crafts.Arcane).Sum() + character.Regalia.Select(s => s.Crafts.Arcane).Sum();
        character.Actuals.Crafts.Alchemy += character.Crafts.Alchemy + character.Inventory.Select(s => s.Crafts.Alchemy).Sum() + character.Regalia.Select(s => s.Crafts.Alchemy).Sum();
        character.Actuals.Crafts.Psionics += character.Crafts.Psionics + character.Inventory.Select(s => s.Crafts.Psionics).Sum() + character.Regalia.Select(s => s.Crafts.Psionics).Sum();
        character.Actuals.Crafts.Hunting += character.Crafts.Hunting + character.Inventory.Select(s => s.Crafts.Hunting).Sum() + character.Regalia.Select(s => s.Crafts.Hunting).Sum();
        character.Actuals.Crafts.Advocacy += character.Crafts.Advocacy + character.Inventory.Select(s => s.Crafts.Advocacy).Sum() + character.Regalia.Select(s => s.Crafts.Advocacy).Sum();
        character.Actuals.Crafts.Mercantile += character.Crafts.Mercantile + character.Inventory.Select(s => s.Crafts.Mercantile).Sum() + character.Regalia.Select(s => s.Crafts.Mercantile).Sum();
        character.Actuals.Crafts.Tactics += character.Crafts.Tactics + character.Inventory.Select(s => s.Crafts.Tactics).Sum() + character.Regalia.Select(s => s.Crafts.Tactics).Sum();
        character.Actuals.Crafts.Traveling += character.Crafts.Traveling + character.Inventory.Select(s => s.Crafts.Traveling).Sum() + character.Regalia.Select(s => s.Crafts.Traveling).Sum();
        character.Actuals.Crafts.Medicine += character.Crafts.Medicine + character.Inventory.Select(s => s.Crafts.Medicine).Sum() + character.Regalia.Select(s => s.Crafts.Medicine).Sum();
        character.Actuals.Crafts.Sailing += character.Crafts.Sailing + character.Inventory.Select(s => s.Crafts.Sailing).Sum() + character.Regalia.Select(s => s.Crafts.Sailing).Sum();
    }

    private static void SetCrafts(CreateCharacter create, Character character)
    {
        // races
        // sets the initial values
        if (create.Race == Statics.Races.Human)
        {
            character.Crafts.Combat       = Statics.Races.Humans.Combat;
            character.Crafts.Arcane       = Statics.Races.Humans.Arcane;
            character.Crafts.Alchemy      = Statics.Races.Humans.Alchemy;
            character.Crafts.Psionics     = Statics.Races.Humans.Psionics;
            character.Crafts.Hunting      = Statics.Races.Humans.Hunting;
            character.Crafts.Advocacy     = Statics.Races.Humans.Advocacy;
            character.Crafts.Mercantile   = Statics.Races.Humans.Mercantile;
            character.Crafts.Tactics      = Statics.Races.Humans.Tactics;
            character.Crafts.Traveling   = Statics.Races.Humans.Traveling;
            character.Crafts.Medicine     = Statics.Races.Humans.Medicine;
            character.Crafts.Sailing      = Statics.Races.Humans.Sail;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Crafts.Combat       = Statics.Races.Elves.Combat;
            character.Crafts.Arcane       = Statics.Races.Elves.Arcane;
            character.Crafts.Alchemy      = Statics.Races.Elves.Alchemy;
            character.Crafts.Psionics     = Statics.Races.Elves.Psionics;
            character.Crafts.Hunting      = Statics.Races.Elves.Hunting;
            character.Crafts.Advocacy     = Statics.Races.Elves.Advocacy;
            character.Crafts.Mercantile   = Statics.Races.Elves.Mercantile;
            character.Crafts.Tactics      = Statics.Races.Elves.Tactics;
            character.Crafts.Traveling   = Statics.Races.Elves.Traveling;
            character.Crafts.Medicine     = Statics.Races.Elves.Medicine;
            character.Crafts.Sailing      = Statics.Races.Elves.Sail;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Crafts.Combat       = Statics.Races.Dwarves.Combat;
            character.Crafts.Arcane       = Statics.Races.Dwarves.Arcane;
            character.Crafts.Alchemy      = Statics.Races.Dwarves.Alchemy;
            character.Crafts.Psionics     = Statics.Races.Dwarves.Psionics;
            character.Crafts.Hunting      = Statics.Races.Dwarves.Hunting;
            character.Crafts.Advocacy     = Statics.Races.Dwarves.Advocacy;
            character.Crafts.Mercantile   = Statics.Races.Dwarves.Mercantile;
            character.Crafts.Tactics      = Statics.Races.Dwarves.Tactics;
            character.Crafts.Traveling   = Statics.Races.Dwarves.Traveling;
            character.Crafts.Medicine     = Statics.Races.Dwarves.Medicine;
            character.Crafts.Sailing      = Statics.Races.Dwarves.Sail;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        // they add to the initial values with +
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Crafts.Combat       += Statics.Cultures.Danarians.Combat;
            character.Crafts.Arcane       += Statics.Cultures.Danarians.Arcane;
            character.Crafts.Alchemy      += Statics.Cultures.Danarians.Alchemy;
            character.Crafts.Psionics     += Statics.Cultures.Danarians.Psionics;
            character.Crafts.Hunting      += Statics.Cultures.Danarians.Hunting;
            character.Crafts.Advocacy     += Statics.Cultures.Danarians.Advocacy;
            character.Crafts.Mercantile   += Statics.Cultures.Danarians.Mercantile;
            character.Crafts.Tactics      += Statics.Cultures.Danarians.Tactics;
            character.Crafts.Traveling   += Statics.Cultures.Danarians.Traveling;
            character.Crafts.Medicine     += Statics.Cultures.Danarians.Medicine;
            character.Crafts.Sailing      += Statics.Cultures.Danarians.Sail;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Crafts.Combat       += Statics.Cultures.Highborns.Combat;
            character.Crafts.Arcane       += Statics.Cultures.Highborns.Arcane;
            character.Crafts.Alchemy      += Statics.Cultures.Highborns.Alchemy;
            character.Crafts.Psionics     += Statics.Cultures.Highborns.Psionics;
            character.Crafts.Hunting      += Statics.Cultures.Highborns.Hunting;
            character.Crafts.Advocacy     += Statics.Cultures.Highborns.Advocacy;
            character.Crafts.Mercantile   += Statics.Cultures.Highborns.Mercantile;
            character.Crafts.Tactics      += Statics.Cultures.Highborns.Tactics;
            character.Crafts.Traveling   += Statics.Cultures.Highborns.Traveling;
            character.Crafts.Medicine     += Statics.Cultures.Highborns.Medicine;
            character.Crafts.Sailing      += Statics.Cultures.Highborns.Sail;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Crafts.Combat       += Statics.Cultures.Undermountains.Combat;
            character.Crafts.Arcane       += Statics.Cultures.Undermountains.Arcane;
            character.Crafts.Alchemy      += Statics.Cultures.Undermountains.Alchemy;
            character.Crafts.Psionics     += Statics.Cultures.Undermountains.Psionics;
            character.Crafts.Hunting      += Statics.Cultures.Undermountains.Hunting;
            character.Crafts.Advocacy     += Statics.Cultures.Undermountains.Advocacy;
            character.Crafts.Mercantile   += Statics.Cultures.Undermountains.Mercantile;
            character.Crafts.Tactics      += Statics.Cultures.Undermountains.Tactics;
            character.Crafts.Traveling   += Statics.Cultures.Undermountains.Traveling;
            character.Crafts.Medicine     += Statics.Cultures.Undermountains.Medicine;
            character.Crafts.Sailing      += Statics.Cultures.Undermountains.Sail;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        // also adds to the initial values with +
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Crafts.Combat       += Statics.Specs.Warrings.Combat;
            character.Crafts.Arcane       += Statics.Specs.Warrings.Arcane;
            character.Crafts.Alchemy      += Statics.Specs.Warrings.Alchemy;
            character.Crafts.Psionics     += Statics.Specs.Warrings.Psionics;
            character.Crafts.Hunting      += Statics.Specs.Warrings.Hunting;
            character.Crafts.Advocacy     += Statics.Specs.Warrings.Advocacy;
            character.Crafts.Mercantile   += Statics.Specs.Warrings.Mercantile;
            character.Crafts.Tactics      += Statics.Specs.Warrings.Tactics;
            character.Crafts.Traveling   += Statics.Specs.Warrings.Traveling;
            character.Crafts.Medicine     += Statics.Specs.Warrings.Medicine;
            character.Crafts.Sailing      += Statics.Specs.Warrings.Sail;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Crafts.Combat       += Statics.Specs.Sorcerys.Combat;
            character.Crafts.Arcane       += Statics.Specs.Sorcerys.Arcane;
            character.Crafts.Alchemy      += Statics.Specs.Sorcerys.Alchemy;
            character.Crafts.Psionics     += Statics.Specs.Sorcerys.Psionics;
            character.Crafts.Hunting      += Statics.Specs.Sorcerys.Hunting;
            character.Crafts.Advocacy     += Statics.Specs.Sorcerys.Advocacy;
            character.Crafts.Mercantile   += Statics.Specs.Sorcerys.Mercantile;
            character.Crafts.Tactics      += Statics.Specs.Sorcerys.Tactics;
            character.Crafts.Traveling   += Statics.Specs.Sorcerys.Traveling;
            character.Crafts.Medicine     += Statics.Specs.Sorcerys.Medicine;
            character.Crafts.Sailing      += Statics.Specs.Sorcerys.Sail;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Crafts.Combat       += Statics.Specs.Trackings.Combat;
            character.Crafts.Arcane       += Statics.Specs.Trackings.Arcane;
            character.Crafts.Alchemy      += Statics.Specs.Trackings.Alchemy;
            character.Crafts.Psionics     += Statics.Specs.Trackings.Psionics;
            character.Crafts.Hunting      += Statics.Specs.Trackings.Hunting;
            character.Crafts.Advocacy     += Statics.Specs.Trackings.Advocacy;
            character.Crafts.Mercantile   += Statics.Specs.Trackings.Mercantile;
            character.Crafts.Tactics      += Statics.Specs.Trackings.Tactics;
            character.Crafts.Traveling   += Statics.Specs.Trackings.Traveling;
            character.Crafts.Medicine     += Statics.Specs.Trackings.Medicine;
            character.Crafts.Sailing      += Statics.Specs.Trackings.Sail;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static void SetStats(CreateCharacter create, Character character)
    {
        // races
        if (create.Race == Statics.Races.Human)
        {
            character.Stats.Strength    = Statics.Races.Humans.Strength;
            character.Stats.Athletics   = Statics.Races.Humans.Athletics;
            character.Stats.Willpower   = Statics.Races.Humans.Willpower;
            character.Stats.Abstract    = Statics.Races.Humans.Abstract;
            character.Stats.Harm        = Statics.Races.Humans.Harm;
            character.Stats.Fortitude   = Statics.Races.Humans.Fortitude;
            character.Stats.Accretion   = Statics.Races.Humans.Accretion;
            character.Stats.Guile       = Statics.Races.Humans.Guile;
            character.Stats.Awareness   = Statics.Races.Humans.Awareness;
            character.Stats.Charm       = Statics.Races.Humans.Charm;
            character.Stats.Hitpoints   = Statics.Races.Humans.Hitpoints;
            character.Stats.Mana        = Statics.Races.Humans.Mana;
            character.Stats.Apcom       = Statics.Races.Humans.Actions;
            character.Stats.Defense     = Statics.Races.Humans.Defense;
            character.Stats.Resist      = Statics.Races.Humans.Resist;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Stats.Strength = Statics.Races.Elves.Strength;
            character.Stats.Athletics = Statics.Races.Elves.Athletics;
            character.Stats.Willpower = Statics.Races.Elves.Willpower;
            character.Stats.Abstract = Statics.Races.Elves.Abstract;
            character.Stats.Harm = Statics.Races.Elves.Harm;
            character.Stats.Fortitude = Statics.Races.Elves.Fortitude;
            character.Stats.Accretion = Statics.Races.Elves.Accretion;
            character.Stats.Guile = Statics.Races.Elves.Guile;
            character.Stats.Awareness = Statics.Races.Elves.Awareness;
            character.Stats.Charm = Statics.Races.Elves.Charm;
            character.Stats.Hitpoints = Statics.Races.Elves.Hitpoints;
            character.Stats.Mana = Statics.Races.Elves.Mana;
            character.Stats.Apcom = Statics.Races.Elves.Actions;
            character.Stats.Defense = Statics.Races.Elves.Defense;
            character.Stats.Resist = Statics.Races.Elves.Resist;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Stats.Strength = Statics.Races.Dwarves.Strength;
            character.Stats.Athletics = Statics.Races.Dwarves.Athletics;
            character.Stats.Willpower = Statics.Races.Dwarves.Willpower;
            character.Stats.Abstract = Statics.Races.Dwarves.Abstract;
            character.Stats.Harm = Statics.Races.Dwarves.Harm;
            character.Stats.Fortitude = Statics.Races.Dwarves.Fortitude;
            character.Stats.Accretion = Statics.Races.Dwarves.Accretion;
            character.Stats.Guile = Statics.Races.Dwarves.Guile;
            character.Stats.Awareness = Statics.Races.Dwarves.Awareness;
            character.Stats.Charm = Statics.Races.Dwarves.Charm;
            character.Stats.Hitpoints = Statics.Races.Dwarves.Hitpoints;
            character.Stats.Mana = Statics.Races.Dwarves.Mana;
            character.Stats.Apcom = Statics.Races.Dwarves.Actions;
            character.Stats.Defense = Statics.Races.Dwarves.Defense;
            character.Stats.Resist = Statics.Races.Dwarves.Resist;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Stats.Strength    += Statics.Cultures.Danarians.Strength;
            character.Stats.Athletics   += Statics.Cultures.Danarians.Athletics;
            character.Stats.Willpower   += Statics.Cultures.Danarians.Willpower;
            character.Stats.Abstract    += Statics.Cultures.Danarians.Abstract;
            character.Stats.Harm        += Statics.Cultures.Danarians.Harm;
            character.Stats.Fortitude   += Statics.Cultures.Danarians.Fortitude;
            character.Stats.Accretion   += Statics.Cultures.Danarians.Accretion;
            character.Stats.Guile       += Statics.Cultures.Danarians.Guile;
            character.Stats.Awareness   += Statics.Cultures.Danarians.Awareness;
            character.Stats.Charm       += Statics.Cultures.Danarians.Charm;
            character.Stats.Hitpoints   += Statics.Cultures.Danarians.Hitpoints;
            character.Stats.Mana        += Statics.Cultures.Danarians.Mana;
            character.Stats.Apcom       += Statics.Cultures.Danarians.Actions;
            character.Stats.Defense     += Statics.Cultures.Danarians.Defense;
            character.Stats.Resist      += Statics.Cultures.Danarians.Resist;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Stats.Strength    += Statics.Cultures.Highborns.Strength;
            character.Stats.Athletics   += Statics.Cultures.Highborns.Athletics;
            character.Stats.Willpower   += Statics.Cultures.Highborns.Willpower;
            character.Stats.Abstract    += Statics.Cultures.Highborns.Abstract;
            character.Stats.Harm        += Statics.Cultures.Highborns.Harm;
            character.Stats.Fortitude   += Statics.Cultures.Highborns.Fortitude;
            character.Stats.Accretion   += Statics.Cultures.Highborns.Accretion;
            character.Stats.Guile       += Statics.Cultures.Highborns.Guile;
            character.Stats.Awareness   += Statics.Cultures.Highborns.Awareness;
            character.Stats.Charm       += Statics.Cultures.Highborns.Charm;
            character.Stats.Hitpoints   += Statics.Cultures.Highborns.Hitpoints;
            character.Stats.Mana        += Statics.Cultures.Highborns.Mana;
            character.Stats.Apcom       += Statics.Cultures.Highborns.Actions;
            character.Stats.Defense     += Statics.Cultures.Highborns.Defense;
            character.Stats.Resist      += Statics.Cultures.Highborns.Resist;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Stats.Strength    += Statics.Cultures.Undermountains.Strength;
            character.Stats.Athletics   += Statics.Cultures.Undermountains.Athletics;
            character.Stats.Willpower   += Statics.Cultures.Undermountains.Willpower;
            character.Stats.Abstract    += Statics.Cultures.Undermountains.Abstract;
            character.Stats.Harm        += Statics.Cultures.Undermountains.Harm;
            character.Stats.Fortitude   += Statics.Cultures.Undermountains.Fortitude;
            character.Stats.Accretion   += Statics.Cultures.Undermountains.Accretion;
            character.Stats.Guile       += Statics.Cultures.Undermountains.Guile;
            character.Stats.Awareness   += Statics.Cultures.Undermountains.Awareness;
            character.Stats.Charm       += Statics.Cultures.Undermountains.Charm;
            character.Stats.Hitpoints   += Statics.Cultures.Undermountains.Hitpoints;
            character.Stats.Mana        += Statics.Cultures.Undermountains.Mana;
            character.Stats.Apcom       += Statics.Cultures.Undermountains.Actions;
            character.Stats.Defense     += Statics.Cultures.Undermountains.Defense;
            character.Stats.Resist      += Statics.Cultures.Undermountains.Resist;
        }
        else
        {
            throw new NotImplementedException();
        }

        // specs
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Stats.Strength    += Statics.Specs.Warrings.Strength;
            character.Stats.Athletics   += Statics.Specs.Warrings.Athletics;
            character.Stats.Willpower   += Statics.Specs.Warrings.Willpower;
            character.Stats.Abstract    += Statics.Specs.Warrings.Abstract;
            character.Stats.Harm        += Statics.Specs.Warrings.Harm;
            character.Stats.Fortitude   += Statics.Specs.Warrings.Fortitude;
            character.Stats.Accretion   += Statics.Specs.Warrings.Accretion;
            character.Stats.Guile       += Statics.Specs.Warrings.Guile;
            character.Stats.Awareness   += Statics.Specs.Warrings.Awareness;
            character.Stats.Charm       += Statics.Specs.Warrings.Charm;
            character.Stats.Hitpoints   += Statics.Specs.Warrings.Hitpoints;
            character.Stats.Mana        += Statics.Specs.Warrings.Mana;
            character.Stats.Apcom       += Statics.Specs.Warrings.Actions;
            character.Stats.Defense     += Statics.Specs.Warrings.Defense;
            character.Stats.Resist      += Statics.Specs.Warrings.Resist;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Stats.Strength    += Statics.Specs.Sorcerys.Strength;
            character.Stats.Athletics   += Statics.Specs.Sorcerys.Athletics;
            character.Stats.Willpower   += Statics.Specs.Sorcerys.Willpower;
            character.Stats.Abstract    += Statics.Specs.Sorcerys.Abstract;
            character.Stats.Harm        += Statics.Specs.Sorcerys.Harm;
            character.Stats.Fortitude   += Statics.Specs.Sorcerys.Fortitude;
            character.Stats.Accretion   += Statics.Specs.Sorcerys.Accretion;
            character.Stats.Guile       += Statics.Specs.Sorcerys.Guile;
            character.Stats.Awareness   += Statics.Specs.Sorcerys.Awareness;
            character.Stats.Charm       += Statics.Specs.Sorcerys.Charm;
            character.Stats.Hitpoints   += Statics.Specs.Sorcerys.Hitpoints;
            character.Stats.Mana        += Statics.Specs.Sorcerys.Mana;
            character.Stats.Apcom       += Statics.Specs.Sorcerys.Actions;
            character.Stats.Defense     += Statics.Specs.Sorcerys.Defense;
            character.Stats.Resist      += Statics.Specs.Sorcerys.Resist;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Stats.Strength    += Statics.Specs.Trackings.Strength;
            character.Stats.Athletics   += Statics.Specs.Trackings.Athletics;
            character.Stats.Willpower   += Statics.Specs.Trackings.Willpower;
            character.Stats.Abstract    += Statics.Specs.Trackings.Abstract;
            character.Stats.Harm        += Statics.Specs.Trackings.Harm;
            character.Stats.Fortitude   += Statics.Specs.Trackings.Fortitude;
            character.Stats.Accretion   += Statics.Specs.Trackings.Accretion;
            character.Stats.Guile       += Statics.Specs.Trackings.Guile;
            character.Stats.Awareness   += Statics.Specs.Trackings.Awareness;
            character.Stats.Charm       += Statics.Specs.Trackings.Charm;
            character.Stats.Hitpoints   += Statics.Specs.Trackings.Hitpoints;
            character.Stats.Mana        += Statics.Specs.Trackings.Mana;
            character.Stats.Apcom       += Statics.Specs.Trackings.Actions;
            character.Stats.Defense     += Statics.Specs.Trackings.Defense;
            character.Stats.Resist      += Statics.Specs.Trackings.Resist;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static void SetDetails(CreateCharacter createCharacter, Character character)
    {
        character.Details.Name = createCharacter.Name;
        character.Details.Portrait = createCharacter.Portrait;
        character.Details.Race = createCharacter.Race;
        character.Details.Culture = createCharacter.Culture;
        character.Details.Spec = createCharacter.Spec;
        character.Details.IsAlive = true;
        character.Details.IsHidden = false;
        character.Details.IsLocked = false;
        character.Details.IsNpc = false;
        character.Details.Entitylevel = 1;
        character.Details.Levelup = 10;
        character.Details.Wealth = 10;
    }

    private void SetInventory(Character character)
    {
        character.Inventory.Add(_items.GenerateSpecificItem(Statics.Items.Types.Weapon));
    }

    private void SetTrinkets(Character character)
    {
        character.Regalia.Add(_items.GenerateRandomTrinket());
    }

    #endregion
}

