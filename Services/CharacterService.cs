using Models;
using Newtonsoft.Json;

namespace Services;

public interface ICharacterService
{
    Character GetCharacter(Guid id, Guid sessionId);
    Character GetCharacterActual(Guid id, Guid sessionId);
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
            Traits = SetTraits(create),
            Details = SetDetails(create),
            Stats = SetStats(create),
            Crafts = SetCrafts(create),
            Assets = SetAssets(create),
        };

        character.Inventory.Add(_items.GenerateSpecificItem(Statics.Items.Types.Weapon));
        character.Trinkets.Add(_items.GenerateRandomTrinket());

        CalculateAssets(character);

        _snapshot.Characters.Add(character);

        var characterEncr = EncryptionService.EncryptString(JsonConvert.SerializeObject(character));

        return characterEncr;
    }

    public Character GetCharacterActual(Guid id, Guid sessionId)
    {
        // TODO: calculate ACTUALS based on items, trinkets and special skills
        throw new NotImplementedException();
    }

    public Character GetCharacter(Guid id, Guid sessionId)
    {
        Validators.ValidateOnGetCharacter(id, sessionId);

        var character = _snapshot.Characters.FirstOrDefault(s => s.Identity.Id == id && s.Identity.SessionId == sessionId);

        Validators.ValidateAgainstNull(character!, "No character found. Go to import first.");

        return character!;
    }

    #region private methods
    private static void CalculateAssets(Character character)
    {
        // hitpoints
        character.Assets.HitpointsBase += 
            character.Stats.Strength
            + character.Stats.Athletics
            + character.Stats.Willpower / 2
            + character.Crafts.Combat
            + character.Crafts.Hunting / 2
            + character.Crafts.Medicine / 3;
        // mana
        character.Assets.ManaBase +=
            character.Stats.Willpower
            + character.Stats.Abstract * 2
            + character.Crafts.Arcane
            + character.Crafts.Psionics / 2
            + character.Crafts.Medicine;
        // actions
        character.Assets.ActionsBase +=
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
        character.Assets.DefenseBase +=
            (character.Stats.Strength
            + character.Stats.Athletics
            + character.Crafts.Combat
            + character.Crafts.Medicine / 2) / 4;
        // resist
        character.Assets.ResistBase +=
            (character.Stats.Strength / 2
            + character.Stats.Abstract / 2
            + character.Crafts.Psionics) / 3;
        // reflex
        character.Assets.ReflexBase +=
            (character.Stats.Athletics
            + character.Stats.Abstract / 2
            + character.Crafts.Hunting) / 3;

        character.Assets.HitpointsActual = character.Assets.HitpointsBase;
        character.Assets.ManaActual = character.Assets.ManaBase;
        character.Assets.ActionsActual = character.Assets.ActionsBase;
        character.Assets.DefenseActual = character.Assets.DefenseBase >= 90 ? 90 : character.Assets.DefenseBase;
        character.Assets.ResistActual = character.Assets.ResistBase;
        character.Assets.ReflexActual = character.Assets.ReflexBase;
    }

    private static CharacterAssets SetAssets(CreateCharacter create)
    {
        var assets = new CharacterAssets();

        // races
        if (create.Race == Statics.Races.Human)
        {
            assets.HitpointsBase    = Statics.Races.Humans.Hitpoints;
            assets.ManaBase         = Statics.Races.Humans.Mana;
            assets.ActionsBase      = Statics.Races.Humans.Actions;
            assets.DefenseBase      = Statics.Races.Humans.Defense;
            assets.ResistBase       = Statics.Races.Humans.Resist;
            assets.ReflexBase       = Statics.Races.Humans.Reflex;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            assets.HitpointsBase    = Statics.Races.Elves.Hitpoints;
            assets.ManaBase         = Statics.Races.Elves.Mana;
            assets.ActionsBase      = Statics.Races.Elves.Actions;
            assets.DefenseBase      = Statics.Races.Elves.Defense;
            assets.ResistBase       = Statics.Races.Elves.Resist;
            assets.ReflexBase       = Statics.Races.Elves.Reflex;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            assets.HitpointsBase    = Statics.Races.Dwarves.Hitpoints;
            assets.ManaBase         = Statics.Races.Dwarves.Mana;
            assets.ActionsBase      = Statics.Races.Dwarves.Actions;
            assets.DefenseBase      = Statics.Races.Dwarves.Defense;
            assets.ResistBase       = Statics.Races.Dwarves.Resist;
            assets.ReflexBase       = Statics.Races.Dwarves.Reflex;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            assets.HitpointsBase    += Statics.Cultures.Danarians.Hitpoints;
            assets.ManaBase         += Statics.Cultures.Danarians.Mana;
            assets.ActionsBase      += Statics.Cultures.Danarians.Actions;
            assets.DefenseBase      += Statics.Cultures.Danarians.Defense;
            assets.ResistBase       += Statics.Cultures.Danarians.Resist;
            assets.ReflexBase       += Statics.Cultures.Danarians.Reflex;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            assets.HitpointsBase    += Statics.Cultures.Highborns.Hitpoints;
            assets.ManaBase         += Statics.Cultures.Highborns.Mana;
            assets.ActionsBase      += Statics.Cultures.Highborns.Actions;
            assets.DefenseBase      += Statics.Cultures.Highborns.Defense;
            assets.ResistBase       += Statics.Cultures.Highborns.Resist;
            assets.ReflexBase       += Statics.Cultures.Highborns.Reflex;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            assets.HitpointsBase    += Statics.Cultures.Undermountains.Hitpoints;
            assets.ManaBase         += Statics.Cultures.Undermountains.Mana;
            assets.ActionsBase      += Statics.Cultures.Undermountains.Actions;
            assets.DefenseBase      += Statics.Cultures.Undermountains.Defense;
            assets.ResistBase       += Statics.Cultures.Undermountains.Resist;
            assets.ReflexBase       += Statics.Cultures.Undermountains.Reflex;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec == Statics.Specs.Warring)
        {
            assets.HitpointsBase    += Statics.Specs.Warrings.Hitpoints;
            assets.ManaBase         += Statics.Specs.Warrings.Mana;
            assets.ActionsBase      += Statics.Specs.Warrings.Actions;
            assets.DefenseBase      += Statics.Specs.Warrings.Defense;
            assets.ResistBase       += Statics.Specs.Warrings.Resist;
            assets.ReflexBase       += Statics.Specs.Warrings.Reflex;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            assets.HitpointsBase    += Statics.Specs.Sorcerys.Hitpoints;
            assets.ManaBase         += Statics.Specs.Sorcerys.Mana;
            assets.ActionsBase      += Statics.Specs.Sorcerys.Actions;
            assets.DefenseBase      += Statics.Specs.Sorcerys.Defense;
            assets.ResistBase       += Statics.Specs.Sorcerys.Resist;
            assets.ReflexBase       += Statics.Specs.Sorcerys.Reflex;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            assets.HitpointsBase    += Statics.Specs.Trackings.Hitpoints;
            assets.ManaBase         += Statics.Specs.Trackings.Mana;
            assets.ActionsBase      += Statics.Specs.Trackings.Actions;
            assets.DefenseBase      += Statics.Specs.Trackings.Defense;
            assets.ResistBase       += Statics.Specs.Trackings.Resist;
            assets.ReflexBase       += Statics.Specs.Trackings.Reflex;
        }
        else
        {
            throw new NotImplementedException();
        }

        return assets;

    }

    private static CharacterCrafts SetCrafts(CreateCharacter create)
    {
        var crafts = new CharacterCrafts();

        // races
        if (create.Race == Statics.Races.Human)
        {
            crafts.Combat       = Statics.Races.Humans.Combat;
            crafts.Arcane       = Statics.Races.Humans.Arcane;
            crafts.Alchemy      = Statics.Races.Humans.Alchemy;
            crafts.Psionics     = Statics.Races.Humans.Psionics;
            crafts.Hunting      = Statics.Races.Humans.Hunting;
            crafts.Advocacy     = Statics.Races.Humans.Advocacy;
            crafts.Mercantile   = Statics.Races.Humans.Mercantile;
            crafts.Tactics      = Statics.Races.Humans.Tactics;
            crafts.Travelling   = Statics.Races.Humans.Travelling;
            crafts.Medicine     = Statics.Races.Humans.Medicine;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            crafts.Combat       = Statics.Races.Elves.Combat;
            crafts.Arcane       = Statics.Races.Elves.Arcane;
            crafts.Alchemy      = Statics.Races.Elves.Alchemy;
            crafts.Psionics     = Statics.Races.Elves.Psionics;
            crafts.Hunting      = Statics.Races.Elves.Hunting;
            crafts.Advocacy     = Statics.Races.Elves.Advocacy;
            crafts.Mercantile   = Statics.Races.Elves.Mercantile;
            crafts.Tactics      = Statics.Races.Elves.Tactics;
            crafts.Travelling   = Statics.Races.Elves.Travelling;
            crafts.Medicine     = Statics.Races.Elves.Medicine;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            crafts.Combat       = Statics.Races.Dwarves.Combat;
            crafts.Arcane       = Statics.Races.Dwarves.Arcane;
            crafts.Alchemy      = Statics.Races.Dwarves.Alchemy;
            crafts.Psionics     = Statics.Races.Dwarves.Psionics;
            crafts.Hunting      = Statics.Races.Dwarves.Hunting;
            crafts.Advocacy     = Statics.Races.Dwarves.Advocacy;
            crafts.Mercantile   = Statics.Races.Dwarves.Mercantile;
            crafts.Tactics      = Statics.Races.Dwarves.Tactics;
            crafts.Travelling   = Statics.Races.Dwarves.Travelling;
            crafts.Medicine     = Statics.Races.Dwarves.Medicine;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            crafts.Combat       += Statics.Cultures.Danarians.Combat;
            crafts.Arcane       += Statics.Cultures.Danarians.Arcane;
            crafts.Alchemy      += Statics.Cultures.Danarians.Alchemy;
            crafts.Psionics     += Statics.Cultures.Danarians.Psionics;
            crafts.Hunting      += Statics.Cultures.Danarians.Hunting;
            crafts.Advocacy     += Statics.Cultures.Danarians.Advocacy;
            crafts.Mercantile   += Statics.Cultures.Danarians.Mercantile;
            crafts.Tactics      += Statics.Cultures.Danarians.Tactics;
            crafts.Travelling   += Statics.Cultures.Danarians.Travelling;
            crafts.Medicine     += Statics.Cultures.Danarians.Medicine;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            crafts.Combat       += Statics.Cultures.Highborns.Combat;
            crafts.Arcane       += Statics.Cultures.Highborns.Arcane;
            crafts.Alchemy      += Statics.Cultures.Highborns.Alchemy;
            crafts.Psionics     += Statics.Cultures.Highborns.Psionics;
            crafts.Hunting      += Statics.Cultures.Highborns.Hunting;
            crafts.Advocacy     += Statics.Cultures.Highborns.Advocacy;
            crafts.Mercantile   += Statics.Cultures.Highborns.Mercantile;
            crafts.Tactics      += Statics.Cultures.Highborns.Tactics;
            crafts.Travelling   += Statics.Cultures.Highborns.Travelling;
            crafts.Medicine     += Statics.Cultures.Highborns.Medicine;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            crafts.Combat       += Statics.Cultures.Undermountains.Combat;
            crafts.Arcane       += Statics.Cultures.Undermountains.Arcane;
            crafts.Alchemy      += Statics.Cultures.Undermountains.Alchemy;
            crafts.Psionics     += Statics.Cultures.Undermountains.Psionics;
            crafts.Hunting      += Statics.Cultures.Undermountains.Hunting;
            crafts.Advocacy     += Statics.Cultures.Undermountains.Advocacy;
            crafts.Mercantile   += Statics.Cultures.Undermountains.Mercantile;
            crafts.Tactics      += Statics.Cultures.Undermountains.Tactics;
            crafts.Travelling   += Statics.Cultures.Undermountains.Travelling;
            crafts.Medicine     += Statics.Cultures.Undermountains.Medicine;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec == Statics.Specs.Warring)
        {
            crafts.Combat       += Statics.Specs.Warrings.Combat;
            crafts.Arcane       += Statics.Specs.Warrings.Arcane;
            crafts.Alchemy      += Statics.Specs.Warrings.Alchemy;
            crafts.Psionics     += Statics.Specs.Warrings.Psionics;
            crafts.Hunting      += Statics.Specs.Warrings.Hunting;
            crafts.Advocacy     += Statics.Specs.Warrings.Advocacy;
            crafts.Mercantile   += Statics.Specs.Warrings.Mercantile;
            crafts.Tactics      += Statics.Specs.Warrings.Tactics;
            crafts.Travelling   += Statics.Specs.Warrings.Travelling;
            crafts.Medicine     += Statics.Specs.Warrings.Medicine;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            crafts.Combat       += Statics.Specs.Sorcerys.Combat;
            crafts.Arcane       += Statics.Specs.Sorcerys.Arcane;
            crafts.Alchemy      += Statics.Specs.Sorcerys.Alchemy;
            crafts.Psionics     += Statics.Specs.Sorcerys.Psionics;
            crafts.Hunting      += Statics.Specs.Sorcerys.Hunting;
            crafts.Advocacy     += Statics.Specs.Sorcerys.Advocacy;
            crafts.Mercantile   += Statics.Specs.Sorcerys.Mercantile;
            crafts.Tactics      += Statics.Specs.Sorcerys.Tactics;
            crafts.Travelling   += Statics.Specs.Sorcerys.Travelling;
            crafts.Medicine     += Statics.Specs.Sorcerys.Medicine;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            crafts.Combat       += Statics.Specs.Trackings.Combat;
            crafts.Arcane       += Statics.Specs.Trackings.Arcane;
            crafts.Alchemy      += Statics.Specs.Trackings.Alchemy;
            crafts.Psionics     += Statics.Specs.Trackings.Psionics;
            crafts.Hunting      += Statics.Specs.Trackings.Hunting;
            crafts.Advocacy     += Statics.Specs.Trackings.Advocacy;
            crafts.Mercantile   += Statics.Specs.Trackings.Mercantile;
            crafts.Tactics      += Statics.Specs.Trackings.Tactics;
            crafts.Travelling   += Statics.Specs.Trackings.Travelling;
            crafts.Medicine     += Statics.Specs.Trackings.Medicine;
        }
        else
        {
            throw new NotImplementedException();
        }

        return crafts;
    }

    private static CharacterStats SetStats(CreateCharacter create)
    {
        var stats = new CharacterStats();

        // races
        if (create.Race == Statics.Races.Human)
        {
            stats.Strength = Statics.Races.Humans.Strength;
            stats.Athletics = Statics.Races.Humans.Athletics;
            stats.Willpower = Statics.Races.Humans.Willpower;
            stats.Abstract = Statics.Races.Humans.Abstract;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            stats.Strength = Statics.Races.Elves.Strength;
            stats.Athletics = Statics.Races.Elves.Athletics;
            stats.Willpower = Statics.Races.Elves.Willpower;
            stats.Abstract = Statics.Races.Elves.Abstract;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            stats.Strength = Statics.Races.Dwarves.Strength;
            stats.Athletics = Statics.Races.Dwarves.Athletics;
            stats.Willpower = Statics.Races.Dwarves.Willpower;
            stats.Abstract = Statics.Races.Dwarves.Abstract;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            stats.Strength  += Statics.Cultures.Danarians.Strength;
            stats.Athletics += Statics.Cultures.Danarians.Athletics;
            stats.Willpower += Statics.Cultures.Danarians.Willpower;
            stats.Abstract  += Statics.Cultures.Danarians.Abstract;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            stats.Strength  += Statics.Cultures.Highborns.Strength;
            stats.Athletics += Statics.Cultures.Highborns.Athletics;
            stats.Willpower += Statics.Cultures.Highborns.Willpower;
            stats.Abstract  += Statics.Cultures.Highborns.Abstract;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            stats.Strength  += Statics.Cultures.Undermountains.Strength;
            stats.Athletics += Statics.Cultures.Undermountains.Athletics;
            stats.Willpower += Statics.Cultures.Undermountains.Willpower;
            stats.Abstract  += Statics.Cultures.Undermountains.Abstract;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec == Statics.Specs.Warring)
        {
            stats.Strength  += Statics.Specs.Warrings.Strength;
            stats.Athletics += Statics.Specs.Warrings.Athletics;
            stats.Willpower += Statics.Specs.Warrings.Willpower;
            stats.Abstract  += Statics.Specs.Warrings.Abstract;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            stats.Strength  += Statics.Specs.Sorcerys.Strength;
            stats.Athletics += Statics.Specs.Sorcerys.Athletics;
            stats.Willpower += Statics.Specs.Sorcerys.Willpower;
            stats.Abstract  += Statics.Specs.Sorcerys.Abstract;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            stats.Strength  += Statics.Specs.Trackings.Strength;
            stats.Athletics += Statics.Specs.Trackings.Athletics;
            stats.Willpower += Statics.Specs.Trackings.Willpower;
            stats.Abstract  += Statics.Specs.Trackings.Abstract;
        }
        else
        {
            throw new NotImplementedException();
        }

        return stats;
    }

    private static CharacterDetails SetDetails(CreateCharacter character)
    {
        return new CharacterDetails
        {
            Entitylevel = 1,
            IsAlive = true,
            IsHidden = false,
            IsLocked = false,
            IsNpc = false,
            Levelup = 10,
            Name = character.Name,
            Portrait = character.Portrait,
            Wealth = 10
        };
    }

    private static CharacterTraits SetTraits(CreateCharacter character)
    {
        return new CharacterTraits
        {
            Race = character.Race,
            Culture = character.Culture,
            Spec = character.Spec,
        };
    }


    #endregion
}

