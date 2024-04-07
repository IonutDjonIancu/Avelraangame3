using Models;

namespace Services;

public interface ICharacterService
{
    Character CreateCharacter(CreateCharacter createCharacter);
    Character GetCharacter(Guid id, Guid sessionId);
}

public class CharacterService(
    ISnapshot snapshot,
    IDiceService diceService,
    IItemService itemService) : ICharacterService
{
    private readonly ISnapshot _snapshot = snapshot;
    private readonly IDiceService _dice = diceService;
    private readonly IItemService _items = itemService;

    public Character GetCharacter(Guid id, Guid sessionId)
    {
        Validators.ValidateOnGetCharacter(id, sessionId);

        var character = _snapshot.Characters.Find(s => s.Identity.Id == id && s.Identity.SessionId == sessionId);

        Validators.ValidateAgainstNull(character!, "No character found");

        return character!;
    }

    public Character CreateCharacter(CreateCharacter create)
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
        };

        character.Items.Add(_items.GenerateSpecificItem(Statics.ItemType.Weapon));

        return character;
    }

    #region private methods
    private static CharacterCrafts SetCrafts(CreateCharacter create)
    {
        var crafts = new CharacterCrafts();

        // races
        if (create.Race.ToLower() == Statics.Races.Human)
        {
            crafts.Combat = Statics.Races.Humans.Combat;
            crafts.Arcane = Statics.Races.Humans.Arcane;
            crafts.Alchemy = Statics.Races.Humans.Alchemy;
            crafts.Psionics = Statics.Races.Humans.Psionics;
            crafts.Hunting = Statics.Races.Humans.Hunting;
            crafts.Advocacy = Statics.Races.Humans.Advocacy;
            crafts.Mercantile = Statics.Races.Humans.Mercantile;
            crafts.Tactics = Statics.Races.Humans.Tactics;
            crafts.Travelling = Statics.Races.Humans.Travelling;
            crafts.Medicine = Statics.Races.Humans.Medicine;
        }
        else if (create.Race.ToLower() == Statics.Races.Elf)
        {
            crafts.Combat = Statics.Races.Elves.Combat;
            crafts.Arcane = Statics.Races.Elves.Arcane;
            crafts.Alchemy = Statics.Races.Elves.Alchemy;
            crafts.Psionics = Statics.Races.Elves.Psionics;
            crafts.Hunting = Statics.Races.Elves.Hunting;
            crafts.Advocacy = Statics.Races.Elves.Advocacy;
            crafts.Mercantile = Statics.Races.Elves.Mercantile;
            crafts.Tactics = Statics.Races.Elves.Tactics;
            crafts.Travelling = Statics.Races.Elves.Travelling;
            crafts.Medicine = Statics.Races.Elves.Medicine;
        }
        else if (create.Race.ToLower() == Statics.Races.Dwarf)
        {
            crafts.Combat = Statics.Races.Dwarves.Combat;
            crafts.Arcane = Statics.Races.Dwarves.Arcane;
            crafts.Alchemy = Statics.Races.Dwarves.Alchemy;
            crafts.Psionics = Statics.Races.Dwarves.Psionics;
            crafts.Hunting = Statics.Races.Dwarves.Hunting;
            crafts.Advocacy = Statics.Races.Dwarves.Advocacy;
            crafts.Mercantile = Statics.Races.Dwarves.Mercantile;
            crafts.Tactics = Statics.Races.Dwarves.Tactics;
            crafts.Travelling = Statics.Races.Dwarves.Travelling;
            crafts.Medicine = Statics.Races.Dwarves.Medicine;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture.ToLower() == Statics.Cultures.Danarian)
        {
            crafts.Combat = Statics.Cultures.Danarians.Combat;
            crafts.Arcane = Statics.Cultures.Danarians.Arcane;
            crafts.Alchemy = Statics.Cultures.Danarians.Alchemy;
            crafts.Psionics = Statics.Cultures.Danarians.Psionics;
            crafts.Hunting = Statics.Cultures.Danarians.Hunting;
            crafts.Advocacy = Statics.Cultures.Danarians.Advocacy;
            crafts.Mercantile = Statics.Cultures.Danarians.Mercantile;
            crafts.Tactics = Statics.Cultures.Danarians.Tactics;
            crafts.Travelling = Statics.Cultures.Danarians.Travelling;
            crafts.Medicine = Statics.Cultures.Danarians.Medicine;
        }
        else if (create.Culture.ToLower() == Statics.Cultures.Highborn)
        {
            crafts.Combat = Statics.Cultures.Highborns.Combat;
            crafts.Arcane = Statics.Cultures.Highborns.Arcane;
            crafts.Alchemy = Statics.Cultures.Highborns.Alchemy;
            crafts.Psionics = Statics.Cultures.Highborns.Psionics;
            crafts.Hunting = Statics.Cultures.Highborns.Hunting;
            crafts.Advocacy = Statics.Cultures.Highborns.Advocacy;
            crafts.Mercantile = Statics.Cultures.Highborns.Mercantile;
            crafts.Tactics = Statics.Cultures.Highborns.Tactics;
            crafts.Travelling = Statics.Cultures.Highborns.Travelling;
            crafts.Medicine = Statics.Cultures.Highborns.Medicine;
        }
        else if (create.Culture.ToLower() == Statics.Cultures.Undermountain)
        {
            crafts.Combat = Statics.Cultures.Undermountains.Combat;
            crafts.Arcane = Statics.Cultures.Undermountains.Arcane;
            crafts.Alchemy = Statics.Cultures.Undermountains.Alchemy;
            crafts.Psionics = Statics.Cultures.Undermountains.Psionics;
            crafts.Hunting = Statics.Cultures.Undermountains.Hunting;
            crafts.Advocacy = Statics.Cultures.Undermountains.Advocacy;
            crafts.Mercantile = Statics.Cultures.Undermountains.Mercantile;
            crafts.Tactics = Statics.Cultures.Undermountains.Tactics;
            crafts.Travelling = Statics.Cultures.Undermountains.Travelling;
            crafts.Medicine = Statics.Cultures.Undermountains.Medicine;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec.ToLower() == Statics.Specs.Warring)
        {
            crafts.Combat = Statics.Specs.Warrings.Combat;
            crafts.Arcane = Statics.Specs.Warrings.Arcane;
            crafts.Alchemy = Statics.Specs.Warrings.Alchemy;
            crafts.Psionics = Statics.Specs.Warrings.Psionics;
            crafts.Hunting = Statics.Specs.Warrings.Hunting;
            crafts.Advocacy = Statics.Specs.Warrings.Advocacy;
            crafts.Mercantile = Statics.Specs.Warrings.Mercantile;
            crafts.Tactics = Statics.Specs.Warrings.Tactics;
            crafts.Travelling = Statics.Specs.Warrings.Travelling;
            crafts.Medicine = Statics.Specs.Warrings.Medicine;
        }
        else if (create.Spec.ToLower() == Statics.Specs.Sorcery)
        {
            crafts.Combat = Statics.Specs.Sorcerys.Combat;
            crafts.Arcane = Statics.Specs.Sorcerys.Arcane;
            crafts.Alchemy = Statics.Specs.Sorcerys.Alchemy;
            crafts.Psionics = Statics.Specs.Sorcerys.Psionics;
            crafts.Hunting = Statics.Specs.Sorcerys.Hunting;
            crafts.Advocacy = Statics.Specs.Sorcerys.Advocacy;
            crafts.Mercantile = Statics.Specs.Sorcerys.Mercantile;
            crafts.Tactics = Statics.Specs.Sorcerys.Tactics;
            crafts.Travelling = Statics.Specs.Sorcerys.Travelling;
            crafts.Medicine = Statics.Specs.Sorcerys.Medicine;
        }
        else if (create.Spec.ToLower() == Statics.Specs.Tracker)
        {
            crafts.Combat = Statics.Specs.Trackers.Combat;
            crafts.Arcane = Statics.Specs.Trackers.Arcane;
            crafts.Alchemy = Statics.Specs.Trackers.Alchemy;
            crafts.Psionics = Statics.Specs.Trackers.Psionics;
            crafts.Hunting = Statics.Specs.Trackers.Hunting;
            crafts.Advocacy = Statics.Specs.Trackers.Advocacy;
            crafts.Mercantile = Statics.Specs.Trackers.Mercantile;
            crafts.Tactics = Statics.Specs.Trackers.Tactics;
            crafts.Travelling = Statics.Specs.Trackers.Travelling;
            crafts.Medicine = Statics.Specs.Trackers.Medicine;
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
        if (create.Race.ToLower() == Statics.Races.Human)
        {
            stats.Strength = Statics.Races.Humans.Strength;
            stats.Athletics = Statics.Races.Humans.Athletics;
            stats.Willpower = Statics.Races.Humans.Willpower;
            stats.Abstract = Statics.Races.Humans.Abstract;
        }
        else if (create.Race.ToLower() == Statics.Races.Elf)
        {
            stats.Strength = Statics.Races.Elves.Strength;
            stats.Athletics = Statics.Races.Elves.Athletics;
            stats.Willpower = Statics.Races.Elves.Willpower;
            stats.Abstract = Statics.Races.Elves.Abstract;
        }
        else if (create.Race.ToLower() == Statics.Races.Dwarf)
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
        if (create.Culture.ToLower() == Statics.Cultures.Danarian)
        {
            stats.Strength += Statics.Cultures.Danarians.Strength;
            stats.Athletics += Statics.Cultures.Danarians.Athletics;
            stats.Willpower += Statics.Cultures.Danarians.Willpower;
            stats.Abstract += Statics.Cultures.Danarians.Abstract;
        }
        else if (create.Culture.ToLower() == Statics.Cultures.Highborn)
        {
            stats.Strength += Statics.Cultures.Highborns.Strength;
            stats.Athletics += Statics.Cultures.Highborns.Athletics;
            stats.Willpower += Statics.Cultures.Highborns.Willpower;
            stats.Abstract += Statics.Cultures.Highborns.Abstract;
        }
        else if (create.Culture.ToLower() == Statics.Cultures.Undermountain)
        {
            stats.Strength += Statics.Cultures.Undermountains.Strength;
            stats.Athletics += Statics.Cultures.Undermountains.Athletics;
            stats.Willpower += Statics.Cultures.Undermountains.Willpower;
            stats.Abstract += Statics.Cultures.Undermountains.Abstract;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec.ToLower() == Statics.Specs.Warring)
        {
            stats.Strength += Statics.Specs.Warrings.Strength;
            stats.Athletics += Statics.Specs.Warrings.Athletics;
            stats.Willpower += Statics.Specs.Warrings.Willpower;
            stats.Abstract += Statics.Specs.Warrings.Abstract;
        }
        else if (create.Spec.ToLower() == Statics.Specs.Sorcery)
        {
            stats.Strength += Statics.Specs.Sorcerys.Strength;
            stats.Athletics += Statics.Specs.Sorcerys.Athletics;
            stats.Willpower += Statics.Specs.Sorcerys.Willpower;
            stats.Abstract += Statics.Specs.Sorcerys.Abstract;
        }
        else if (create.Spec.ToLower() == Statics.Specs.Tracker)
        {
            stats.Strength += Statics.Specs.Trackers.Strength;
            stats.Athletics += Statics.Specs.Trackers.Athletics;
            stats.Willpower += Statics.Specs.Trackers.Willpower;
            stats.Abstract += Statics.Specs.Trackers.Abstract;
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
            Levelup = 10,
            Name = character.Name,
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

