using Models;
using Newtonsoft.Json;

namespace Services;

public interface ICharacterService
{
    CharacterVm GetCharacter(Guid id, Guid sessionId);
    CharacterVm GetCharacterActual(Guid id, Guid sessionId);
    string CreateCharacter(CreateCharacter createCharacter);
    ImportCharacterResponse ImportCharacter(ImportCharacter characterString);
}

public class CharacterService : ICharacterService
{
    private readonly ISnapshot _snapshot;
    private readonly IItemService _items;

    public CharacterService(
        ISnapshot snapshot,
        IItemService itemService)
    {
        _snapshot = snapshot;
        _items = itemService;
    }

    public ImportCharacterResponse ImportCharacter(ImportCharacter import)
    {
        Validators.ValidateOnImportCharacter(import);
        var decryptString = EncryptionService.DecryptString(import.CharacterString);

        var character = JsonConvert.DeserializeObject<Character>(decryptString)!;

        var oldChar = _snapshot.Characters.FirstOrDefault(s => s.Identity.Id == character.Identity.Id);

        if (oldChar != null)
            _snapshot.Characters.Remove(oldChar);
        
        _snapshot.Characters.Add(character);

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
        SetAbilities(create, character);
        SetAssets(create, character);
        CalculateAssets(character);

        SetInventory(character);
        SetTrinkets(character);

        character.Supplies.Items.AddRange(_items.GenerateRandomItems(3)); // TODO: remove

        _snapshot.Characters.Add(character);

        var characterEncr = EncryptionService.EncryptString(JsonConvert.SerializeObject(character));

        return characterEncr;
    }

    public CharacterVm GetCharacterActual(Guid id, Guid sessionId)
    {
        // TODO: calculate ACTUALS based on items, trinkets and special skills
        throw new NotImplementedException();
    }

    public CharacterVm GetCharacter(Guid id, Guid sessionId)
    {
        Validators.ValidateOnGetCharacter(id, sessionId);

        var character = _snapshot.Characters.FirstOrDefault(s => s.Identity.Id == id && s.Identity.SessionId == sessionId);

        Validators.ValidateAgainstNull(character!, "No character found. Go to import first.");

        return new CharacterVm
        {
            Identity = new CharacterIdentityVm
            {
                Id = id,
            },
            Details = character!.Details,
            Stats = character.Stats,
            Assets = character.Assets,
            Crafts = character.Crafts,
            Inventory = character.Inventory,
            Trinkets = character.Trinkets,
            Supplies = character.Supplies
        };
    }

    #region private methods
    private static void CalculateAssets(Character character)
    {
        // hitpoints
        character.Assets.Hitpoints += 
            character.Stats.Strength
            + character.Stats.Athletics
            + character.Stats.Willpower / 2
            + character.Crafts.Combat
            + character.Crafts.Hunting / 2
            + character.Crafts.Medicine / 3;
        // mana
        character.Assets.Mana +=
            character.Stats.Willpower
            + character.Stats.Abstract * 2
            + character.Crafts.Arcane
            + character.Crafts.Psionics / 2
            + character.Crafts.Medicine;
        // actions
        character.Assets.Apcom +=
            (character.Stats.Strength
            + character.Stats.Athletics
            + character.Stats.Willpower
            + character.Stats.Abstract
            + character.Crafts.Combat / 2
            + character.Crafts.Arcane / 2
            + character.Crafts.Psionics / 2
            + character.Crafts.Hunting
            + character.Crafts.Tactics
            + character.Crafts.Medicine) / 11;
        // defense
        character.Assets.Defense +=
            (character.Stats.Strength
            + character.Stats.Athletics
            + character.Crafts.Combat
            + character.Crafts.Medicine / 2) / 4;
        // resist
        character.Assets.Resist +=
            (character.Stats.Strength / 2
            + character.Stats.Abstract / 2
            + character.Crafts.Psionics) / 3;

        character.Assets.HitpointsActual = character.Assets.Hitpoints;
        character.Assets.ManaActual = character.Assets.Mana;
        character.Assets.ApcomActual = character.Assets.Apcom;
        character.Assets.DefenseActual = character.Assets.Defense >= 90 ? 90 : character.Assets.Defense;
        character.Assets.ResistActual = character.Assets.Resist;
    }

    private static void SetAbilities(CreateCharacter create, Character character)
    {
        // races
        if (create.Race == Statics.Races.Human)
        {
            character.Abilities.Harm        = Statics.Races.Humans.Harm;
            character.Abilities.Fortitude   = Statics.Races.Humans.Fortitude;
            character.Abilities.Accretion   = Statics.Races.Humans.Accretion;
            character.Abilities.Guile       = Statics.Races.Humans.Guile;
            character.Abilities.Awareness   = Statics.Races.Humans.Awareness;
            character.Abilities.Charm       = Statics.Races.Humans.Charm;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Abilities.Harm        = Statics.Races.Elves.Harm;
            character.Abilities.Fortitude   = Statics.Races.Elves.Fortitude;
            character.Abilities.Accretion   = Statics.Races.Elves.Accretion;
            character.Abilities.Guile       = Statics.Races.Elves.Guile;
            character.Abilities.Awareness   = Statics.Races.Elves.Awareness;
            character.Abilities.Charm       = Statics.Races.Elves.Charm;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Abilities.Harm        = Statics.Races.Dwarves.Harm;
            character.Abilities.Fortitude   = Statics.Races.Dwarves.Fortitude;
            character.Abilities.Accretion   = Statics.Races.Dwarves.Accretion;
            character.Abilities.Guile       = Statics.Races.Dwarves.Guile;
            character.Abilities.Awareness   = Statics.Races.Dwarves.Awareness;
            character.Abilities.Charm       = Statics.Races.Dwarves.Charm;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Abilities.Harm        += Statics.Cultures.Danarians.Harm;
            character.Abilities.Fortitude   += Statics.Cultures.Danarians.Fortitude;
            character.Abilities.Accretion   += Statics.Cultures.Danarians.Accretion;
            character.Abilities.Guile       += Statics.Cultures.Danarians.Guile;
            character.Abilities.Awareness   += Statics.Cultures.Danarians.Awareness;
            character.Abilities.Charm       += Statics.Cultures.Danarians.Charm;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Abilities.Harm        += Statics.Cultures.Highborns.Harm;
            character.Abilities.Fortitude   += Statics.Cultures.Highborns.Fortitude;
            character.Abilities.Accretion   += Statics.Cultures.Highborns.Accretion;
            character.Abilities.Guile       += Statics.Cultures.Highborns.Guile;
            character.Abilities.Awareness   += Statics.Cultures.Highborns.Awareness;
            character.Abilities.Charm       += Statics.Cultures.Highborns.Charm;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Abilities.Harm        += Statics.Cultures.Undermountains.Harm;
            character.Abilities.Fortitude   += Statics.Cultures.Undermountains.Fortitude;
            character.Abilities.Accretion   += Statics.Cultures.Undermountains.Accretion;
            character.Abilities.Guile       += Statics.Cultures.Undermountains.Guile;
            character.Abilities.Awareness   += Statics.Cultures.Undermountains.Awareness;
            character.Abilities.Charm       += Statics.Cultures.Undermountains.Charm;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Abilities.Harm        += Statics.Specs.Warrings.Harm;
            character.Abilities.Fortitude   += Statics.Specs.Warrings.Fortitude;
            character.Abilities.Accretion   += Statics.Specs.Warrings.Accretion;
            character.Abilities.Guile       += Statics.Specs.Warrings.Guile;
            character.Abilities.Awareness   += Statics.Specs.Warrings.Awareness;
            character.Abilities.Charm       += Statics.Specs.Warrings.Charm;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Abilities.Harm        += Statics.Specs.Sorcerys.Harm;
            character.Abilities.Fortitude   += Statics.Specs.Sorcerys.Fortitude;
            character.Abilities.Accretion   += Statics.Specs.Sorcerys.Accretion;
            character.Abilities.Guile       += Statics.Specs.Sorcerys.Guile;
            character.Abilities.Awareness   += Statics.Specs.Sorcerys.Awareness;
            character.Abilities.Charm       += Statics.Specs.Sorcerys.Charm;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Abilities.Harm        += Statics.Specs.Trackings.Harm;
            character.Abilities.Fortitude   += Statics.Specs.Trackings.Fortitude;
            character.Abilities.Accretion   += Statics.Specs.Trackings.Accretion;
            character.Abilities.Guile       += Statics.Specs.Trackings.Guile;
            character.Abilities.Awareness   += Statics.Specs.Trackings.Awareness;
            character.Abilities.Charm       += Statics.Specs.Trackings.Charm;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static void SetAssets(CreateCharacter create, Character character)
    {
        // races
        if (create.Race == Statics.Races.Human)
        {
            character.Assets.Hitpoints  = Statics.Races.Humans.Hitpoints;
            character.Assets.Mana       = Statics.Races.Humans.Mana;
            character.Assets.Apcom      = Statics.Races.Humans.Actions;
            character.Assets.Defense    = Statics.Races.Humans.Defense;
            character.Assets.Resist     = Statics.Races.Humans.Resist;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Assets.Hitpoints  = Statics.Races.Elves.Hitpoints;
            character.Assets.Mana       = Statics.Races.Elves.Mana;
            character.Assets.Apcom      = Statics.Races.Elves.Actions;
            character.Assets.Defense    = Statics.Races.Elves.Defense;
            character.Assets.Resist     = Statics.Races.Elves.Resist;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Assets.Hitpoints  = Statics.Races.Dwarves.Hitpoints;
            character.Assets.Mana       = Statics.Races.Dwarves.Mana;
            character.Assets.Apcom      = Statics.Races.Dwarves.Actions;
            character.Assets.Defense    = Statics.Races.Dwarves.Defense;
            character.Assets.Resist     = Statics.Races.Dwarves.Resist;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Assets.Hitpoints  += Statics.Cultures.Danarians.Hitpoints;
            character.Assets.Mana       += Statics.Cultures.Danarians.Mana;
            character.Assets.Apcom      += Statics.Cultures.Danarians.Actions;
            character.Assets.Defense    += Statics.Cultures.Danarians.Defense;
            character.Assets.Resist     += Statics.Cultures.Danarians.Resist;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Assets.Hitpoints  += Statics.Cultures.Highborns.Hitpoints;
            character.Assets.Mana       += Statics.Cultures.Highborns.Mana;
            character.Assets.Apcom      += Statics.Cultures.Highborns.Actions;
            character.Assets.Defense    += Statics.Cultures.Highborns.Defense;
            character.Assets.Resist     += Statics.Cultures.Highborns.Resist;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Assets.Hitpoints  += Statics.Cultures.Undermountains.Hitpoints;
            character.Assets.Mana       += Statics.Cultures.Undermountains.Mana;
            character.Assets.Apcom      += Statics.Cultures.Undermountains.Actions;
            character.Assets.Defense    += Statics.Cultures.Undermountains.Defense;
            character.Assets.Resist     += Statics.Cultures.Undermountains.Resist;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Assets.Hitpoints  += Statics.Specs.Warrings.Hitpoints;
            character.Assets.Mana       += Statics.Specs.Warrings.Mana;
            character.Assets.Apcom      += Statics.Specs.Warrings.Actions;
            character.Assets.Defense    += Statics.Specs.Warrings.Defense;
            character.Assets.Resist     += Statics.Specs.Warrings.Resist;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Assets.Hitpoints  += Statics.Specs.Sorcerys.Hitpoints;
            character.Assets.Mana       += Statics.Specs.Sorcerys.Mana;
            character.Assets.Apcom      += Statics.Specs.Sorcerys.Actions;
            character.Assets.Defense    += Statics.Specs.Sorcerys.Defense;
            character.Assets.Resist     += Statics.Specs.Sorcerys.Resist;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Assets.Hitpoints  += Statics.Specs.Trackings.Hitpoints;
            character.Assets.Mana       += Statics.Specs.Trackings.Mana;
            character.Assets.Apcom      += Statics.Specs.Trackings.Actions;
            character.Assets.Defense    += Statics.Specs.Trackings.Defense;
            character.Assets.Resist     += Statics.Specs.Trackings.Resist;
        }
        else
        {
            throw new NotImplementedException();
        }
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
            character.Crafts.Travelling   = Statics.Races.Humans.Travelling;
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
            character.Crafts.Travelling   = Statics.Races.Elves.Travelling;
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
            character.Crafts.Travelling   = Statics.Races.Dwarves.Travelling;
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
            character.Crafts.Travelling   += Statics.Cultures.Danarians.Travelling;
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
            character.Crafts.Travelling   += Statics.Cultures.Highborns.Travelling;
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
            character.Crafts.Travelling   += Statics.Cultures.Undermountains.Travelling;
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
            character.Crafts.Travelling   += Statics.Specs.Warrings.Travelling;
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
            character.Crafts.Travelling   += Statics.Specs.Sorcerys.Travelling;
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
            character.Crafts.Travelling   += Statics.Specs.Trackings.Travelling;
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
            character.Stats.Strength = Statics.Races.Humans.Strength;
            character.Stats.Athletics = Statics.Races.Humans.Athletics;
            character.Stats.Willpower = Statics.Races.Humans.Willpower;
            character.Stats.Abstract = Statics.Races.Humans.Abstract;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Stats.Strength = Statics.Races.Elves.Strength;
            character.Stats.Athletics = Statics.Races.Elves.Athletics;
            character.Stats.Willpower = Statics.Races.Elves.Willpower;
            character.Stats.Abstract = Statics.Races.Elves.Abstract;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Stats.Strength = Statics.Races.Dwarves.Strength;
            character.Stats.Athletics = Statics.Races.Dwarves.Athletics;
            character.Stats.Willpower = Statics.Races.Dwarves.Willpower;
            character.Stats.Abstract = Statics.Races.Dwarves.Abstract;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Stats.Strength  += Statics.Cultures.Danarians.Strength;
            character.Stats.Athletics += Statics.Cultures.Danarians.Athletics;
            character.Stats.Willpower += Statics.Cultures.Danarians.Willpower;
            character.Stats.Abstract  += Statics.Cultures.Danarians.Abstract;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Stats.Strength  += Statics.Cultures.Highborns.Strength;
            character.Stats.Athletics += Statics.Cultures.Highborns.Athletics;
            character.Stats.Willpower += Statics.Cultures.Highborns.Willpower;
            character.Stats.Abstract  += Statics.Cultures.Highborns.Abstract;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Stats.Strength  += Statics.Cultures.Undermountains.Strength;
            character.Stats.Athletics += Statics.Cultures.Undermountains.Athletics;
            character.Stats.Willpower += Statics.Cultures.Undermountains.Willpower;
            character.Stats.Abstract  += Statics.Cultures.Undermountains.Abstract;
        }
        else
        {
            throw new NotImplementedException();
        }

        // specs
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Stats.Strength  += Statics.Specs.Warrings.Strength;
            character.Stats.Athletics += Statics.Specs.Warrings.Athletics;
            character.Stats.Willpower += Statics.Specs.Warrings.Willpower;
            character.Stats.Abstract  += Statics.Specs.Warrings.Abstract;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Stats.Strength  += Statics.Specs.Sorcerys.Strength;
            character.Stats.Athletics += Statics.Specs.Sorcerys.Athletics;
            character.Stats.Willpower += Statics.Specs.Sorcerys.Willpower;
            character.Stats.Abstract  += Statics.Specs.Sorcerys.Abstract;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Stats.Strength  += Statics.Specs.Trackings.Strength;
            character.Stats.Athletics += Statics.Specs.Trackings.Athletics;
            character.Stats.Willpower += Statics.Specs.Trackings.Willpower;
            character.Stats.Abstract  += Statics.Specs.Trackings.Abstract;
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
        character.Details.Levelup = 10;
        character.Details.Wealth = 10;
    }

    private void SetInventory(Character character)
    {
        character.Inventory.Add(_items.GenerateSpecificItem(Statics.Items.Types.Weapon));
    }

    private void SetTrinkets(Character character)
    {
        character.Trinkets.Add(_items.GenerateRandomTrinket());
    }

    #endregion
}

