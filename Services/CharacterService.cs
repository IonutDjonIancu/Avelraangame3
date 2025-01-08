using Models;

namespace Services;

public interface ICharacterService
{
    /// <summary>
    /// Includes character actuals.
    /// </summary>
    /// <param name="identity"></param>
    /// <returns></returns>
    Character GetCharacter(CharacterIdentity identity);
    Characters GetAllCharacters(Guid playerId);
    Characters GetAllLockedCharacters(Guid playerId);
    Characters GetAllDuelistCharacters(Guid playerId);

    void CreateCharacter(CreateCharacter create);
    void DeleteCharacter(CharacterIdentity identity);

    void EquipItem(EquipItem equipItem);
    void UnequipItem(EquipItem equipItem);
    void SellItem(EquipItem equipItem);
    void Levelup(CharacterLevelup levelup);
    void BuyItemFromTown(EquipItem equipItem);

    void SetCharacterFights(Character character);
}

public class CharacterService : ICharacterService
{
    private readonly ISnapshot _snapshot;
    private readonly IValidatorService _validator;
    private readonly IItemService _items;
    private readonly IDiceService _dice;

    public CharacterService(
        ISnapshot snapshot,
        IValidatorService validatorService,
        IItemService itemService,
        IDiceService diceService)
    {
        _snapshot = snapshot;
        _validator = validatorService;
        _items = itemService;
        _dice = diceService;
    }

    public void CreateCharacter(CreateCharacter create)
    {
        _validator.ValidateOnCreateCharacter(create);

        var character = new Character
        {
            Identity = new CharacterIdentity
            {
                Id = Guid.NewGuid(),
                PlayerId = create.PlayerId,
            },
        };

        SetDetails(create, character);
        SetStats(create, character);
        SetInventory(character);
        SetWorth(character);

        _snapshot.Players.First(s => s.Id == create.PlayerId).Characters.Add(character);
    }

    public void DeleteCharacter(CharacterIdentity identity)
    {
        var character = _validator.ValidateCharacterExists(identity)!;

        _snapshot.Players.First(s => s.Id == identity.PlayerId).Characters.Remove(character);
    }

    public Characters GetAllCharacters(Guid playerId)
    {
        _validator.ValidateOnGetCharacters(playerId);

        var listOfCharacters = new Characters();

        //var allPlayerCharacters = _snapshot.Players.First(s => s.Id == playerId).Characters.Where(s => !s.Details.IsNpc).ToList();

        //if (allPlayerCharacters is null)
        //    return listOfCharacters;

        //foreach (var character in allPlayerCharacters)
        //{
        //    if (!character.Details.IsNpc)
        //        listOfCharacters.CharactersList.Add(new CharacterVm
        //        {
        //            Id = character.Identity.Id,
        //            Name = character.Details.Name,
        //            Portrait = character.Details.Portrait
        //        });
        //}

        _snapshot.Players.First(s => s.Id == playerId)
            .Characters
            .Where(s => !s.Details.IsNpc).ToList()
            .ForEach(s =>
            {
                listOfCharacters.CharactersList.Add(new CharacterVm
                {
                    Id = s.Identity.Id,
                    Name = s.Details.Name,
                    Portrait = s.Details.Portrait
                });
            });

        return listOfCharacters;
    }

    public Characters GetAllLockedCharacters(Guid playerId)
    {
        _validator.ValidateOnGetCharacters(playerId);

        var listOfCharacters = new Characters();
        var allPlayerCharacters = _snapshot.Players.First(s => s.Id == playerId).Characters.Where(s => !s.Details.IsNpc && s.Details.IsAlive && s.Details.IsLocked).ToList();

        if (allPlayerCharacters is null)
            return listOfCharacters;

        foreach (var character in allPlayerCharacters)
        {
            if (!character.Details.IsNpc && character.Details.IsAlive && character.Details.IsLocked)
                listOfCharacters.CharactersList.Add(new CharacterVm
                {
                    Id = character.Identity.Id,
                    Name = character.Details.Name,
                    Portrait = character.Details.Portrait
                });
        }
        //_snapshot.Players.First(s => s.Id == playerId)
        //    .Characters
        //    .Where(s => !s.Details.IsNpc && s.Details.IsAlive && s.Details.IsLocked).ToList()
        //    .ForEach(s =>
        //    {
        //        listOfCharacters.CharactersList.Add(new CharacterVm
        //        {
        //            Id = s.Identity.Id,
        //            Name = s.Details.Name,
        //            Portrait = s.Details.Portrait
        //        });
        //    });

        return listOfCharacters;
    }

    public Characters GetAllDuelistCharacters(Guid playerId)
    {
        _validator.ValidateOnGetCharacters(playerId);

        var listOfCharacters = new Characters();

        _snapshot.Players.First(s => s.Id == playerId)
            .Characters
            .Where(s => !s.Details.IsNpc && s.Details.BoardType == Statics.Boards.Types.Duel).ToList()
            .ForEach(s =>
            {
                listOfCharacters.CharactersList.Add(new CharacterVm
                {
                    Id = s.Identity.Id,
                    Name = s.Details.Name,
                    Portrait = s.Details.Portrait
                });
            });

        return listOfCharacters;
    }

    public Character GetCharacter(CharacterIdentity identity)
    {
        var character = _validator.ValidateOnGetCharacter(identity);

        CalculateActuals(character);
        SetWorth(character);

        return character;
    }

    public void EquipItem(EquipItem equipItem)
    {
        var (item, character) = _validator.ValidateEquipItem(equipItem);

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
    }

    public void UnequipItem(EquipItem equipItem)
    {
        var (item, character) = _validator.ValidateUnequipItem(equipItem);

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
    }

    public void SellItem(EquipItem equipItem)
    {
        var (item, character) = _validator.ValidateSellItem(equipItem);

        character.Details.Wealth += item.Value;

        if (item.Type == Statics.Items.Types.Trinket)
        {
            character.Supplies.Regalia.Remove(item as Trinket);
        }
        else
        {
            character.Supplies.Items.Remove(item);
        }

        item.Value += (int)(item.Value * 0.25);

        _snapshot.MarketItems.Add(item);
    }

    public void BuyItemFromTown(EquipItem equipItem)
    {
        lock (this)
        {
            var (item, character) = _validator.ValidateBuyItem(equipItem);

            if (item.Type == Statics.Items.Types.Trinket)
            {
                character.Supplies.Regalia.Add(item as Trinket);
            }
            else
            {
                character.Supplies.Items.Add(item);
            }

            var effort = _dice.Roll1dN(Statics.EffortLevels.Easy);
            var roll = _dice.Rolld20Character(character, Statics.Stats.Social);

            character.Details.Wealth -= roll > effort ? item.Value - (int)(item.Value * 0.25) : item.Value;
            
            _snapshot.MarketItems.Remove(item);
        }
    }

    public void Levelup(CharacterLevelup levelup)
    {
        var character = _validator.ValidateLevelupAndReturn(levelup);

        var stat = typeof(CharacterStats).GetProperty(levelup.Stat)!;
        var value = (int)stat.GetValue(character.Stats.Base)!;
        int valueToAdd;

        if (value <= 0)
        {
            valueToAdd = 1;
        }
        else
        {
            valueToAdd = value + 1;
        }

        stat.SetValue(character.Stats.Base, valueToAdd);

        character.Details.Levelup -= valueToAdd;
    }

    public void SetCharacterFights(Character character)
    {
        // main
        character.Stats.Fight.Strength      = character.Stats.Actual.Strength;
        character.Stats.Fight.Constitution  = character.Stats.Actual.Constitution;
        character.Stats.Fight.Agility       = character.Stats.Actual.Agility;
        character.Stats.Fight.Willpower     = character.Stats.Actual.Willpower;
        character.Stats.Fight.Abstract      = character.Stats.Actual.Abstract;
        // skills
        character.Stats.Fight.Melee         = character.Stats.Actual.Melee;
        character.Stats.Fight.Arcane        = character.Stats.Actual.Arcane;
        character.Stats.Fight.Psionics      = character.Stats.Actual.Psionics;
        character.Stats.Fight.Social        = character.Stats.Actual.Social;
        character.Stats.Fight.Hide          = character.Stats.Actual.Hide;
        character.Stats.Fight.Survival      = character.Stats.Actual.Survival;
        character.Stats.Fight.Tactics       = character.Stats.Actual.Tactics;
        character.Stats.Fight.Aid           = character.Stats.Actual.Aid;
        character.Stats.Fight.Crafting      = character.Stats.Actual.Crafting;
        character.Stats.Fight.Perception          = character.Stats.Actual.Perception;
        // assets
        character.Stats.Fight.Defense       = character.Stats.Actual.Defense;
        character.Stats.Fight.Actions       = character.Stats.Actual.Actions;
        character.Stats.Fight.Hitpoints     = character.Stats.Actual.Hitpoints;
        character.Stats.Fight.Mana          = character.Stats.Actual.Mana;
    }

    #region private methods
    private static void CalculateActuals(Character character)
    {
        // stats
        character.Stats.Actual.Strength     = character.Stats.Base.Strength     + character.Inventory.Select(s => s.Bonuses.Strength).Sum()      + character.Regalia.Select(s => s.Bonuses.Strength).Sum();
        character.Stats.Actual.Constitution = character.Stats.Base.Constitution + character.Inventory.Select(s => s.Bonuses.Constitution).Sum()  + character.Regalia.Select(s => s.Bonuses.Constitution).Sum();
        character.Stats.Actual.Agility      = character.Stats.Base.Agility      + character.Inventory.Select(s => s.Bonuses.Agility).Sum()       + character.Regalia.Select(s => s.Bonuses.Agility).Sum();
        character.Stats.Actual.Willpower    = character.Stats.Base.Willpower    + character.Inventory.Select(s => s.Bonuses.Willpower).Sum()     + character.Regalia.Select(s => s.Bonuses.Willpower).Sum();
        character.Stats.Actual.Abstract     = character.Stats.Base.Abstract     + character.Inventory.Select(s => s.Bonuses.Abstract).Sum()      + character.Regalia.Select(s => s.Bonuses.Abstract).Sum();
        // skills
        character.Stats.Actual.Melee        = character.Stats.Base.Melee        + character.Inventory.Select(s => s.Bonuses.Melee).Sum()         + character.Regalia.Select(s => s.Bonuses.Melee).Sum();
        character.Stats.Actual.Arcane       = character.Stats.Base.Arcane       + character.Inventory.Select(s => s.Bonuses.Arcane).Sum()        + character.Regalia.Select(s => s.Bonuses.Arcane).Sum();
        character.Stats.Actual.Psionics     = character.Stats.Base.Psionics     + character.Inventory.Select(s => s.Bonuses.Psionics).Sum()      + character.Regalia.Select(s => s.Bonuses.Psionics).Sum();
        character.Stats.Actual.Social       = character.Stats.Base.Social       + character.Inventory.Select(s => s.Bonuses.Social).Sum()        + character.Regalia.Select(s => s.Bonuses.Social).Sum();
        character.Stats.Actual.Hide         = character.Stats.Base.Hide         + character.Inventory.Select(s => s.Bonuses.Hide).Sum()          + character.Regalia.Select(s => s.Bonuses.Hide).Sum();
        character.Stats.Actual.Survival     = character.Stats.Base.Survival     + character.Inventory.Select(s => s.Bonuses.Survival).Sum()      + character.Regalia.Select(s => s.Bonuses.Survival).Sum();
        character.Stats.Actual.Tactics      = character.Stats.Base.Tactics      + character.Inventory.Select(s => s.Bonuses.Tactics).Sum()       + character.Regalia.Select(s => s.Bonuses.Tactics).Sum();
        character.Stats.Actual.Aid          = character.Stats.Base.Aid          + character.Inventory.Select(s => s.Bonuses.Aid).Sum()           + character.Regalia.Select(s => s.Bonuses.Aid).Sum();
        character.Stats.Actual.Crafting     = character.Stats.Base.Crafting     + character.Inventory.Select(s => s.Bonuses.Crafting).Sum()      + character.Regalia.Select(s => s.Bonuses.Crafting).Sum();
        character.Stats.Actual.Perception         = character.Stats.Base.Perception         + character.Inventory.Select(s => s.Bonuses.Perception).Sum()          + character.Regalia.Select(s => s.Bonuses.Perception).Sum();
        // assets
        character.Stats.Actual.Defense      = character.Stats.Base.Defense      + character.Inventory.Select(s => s.Bonuses.Defense).Sum()       + character.Regalia.Select(s => s.Bonuses.Defense).Sum();
        character.Stats.Actual.Actions      = character.Stats.Base.Actions      + character.Inventory.Select(s => s.Bonuses.Actions).Sum()       + character.Regalia.Select(s => s.Bonuses.Actions).Sum();
        character.Stats.Actual.Hitpoints    = character.Stats.Base.Hitpoints    + character.Inventory.Select(s => s.Bonuses.Hitpoints).Sum()     + character.Regalia.Select(s => s.Bonuses.Hitpoints).Sum();
        character.Stats.Actual.Mana         = character.Stats.Base.Mana         + character.Inventory.Select(s => s.Bonuses.Mana).Sum()          + character.Regalia.Select(s => s.Bonuses.Mana).Sum();
    }

    private static void SetStats(CreateCharacter create, Character character)
    {
        // races
        if (create.Race == Statics.Races.Human)
        {
            // main
            character.Stats.Base.Strength       += Statics.Races.Humans.Strength;
            character.Stats.Base.Constitution   += Statics.Races.Humans.Constitution;
            character.Stats.Base.Agility        += Statics.Races.Humans.Agility;
            character.Stats.Base.Willpower      += Statics.Races.Humans.Willpower;
            character.Stats.Base.Abstract       += Statics.Races.Humans.Abstract;
            // skills                                            Humans
            character.Stats.Base.Melee          += Statics.Races.Humans.Melee;
            character.Stats.Base.Arcane         += Statics.Races.Humans.Arcane;
            character.Stats.Base.Psionics       += Statics.Races.Humans.Psionics;
            character.Stats.Base.Social         += Statics.Races.Humans.Social;
            character.Stats.Base.Hide           += Statics.Races.Humans.Hide;
            character.Stats.Base.Survival       += Statics.Races.Humans.Survival;
            character.Stats.Base.Tactics        += Statics.Races.Humans.Tactics;
            character.Stats.Base.Aid            += Statics.Races.Humans.Aid;
            character.Stats.Base.Crafting       += Statics.Races.Humans.Crafting;
            character.Stats.Base.Perception           += Statics.Races.Humans.Perception;
            // assets                                            Humans
            character.Stats.Base.Defense        += Statics.Races.Humans.Defense;
            character.Stats.Base.Actions        += Statics.Races.Humans.Actions;
            character.Stats.Base.Hitpoints      += Statics.Races.Humans.Hitpoints;
            character.Stats.Base.Mana           += Statics.Races.Humans.Mana;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            // main
            character.Stats.Base.Strength       += Statics.Races.Elfs.Strength;
            character.Stats.Base.Constitution   += Statics.Races.Elfs.Constitution;
            character.Stats.Base.Agility        += Statics.Races.Elfs.Agility;
            character.Stats.Base.Willpower      += Statics.Races.Elfs.Willpower;
            character.Stats.Base.Abstract       += Statics.Races.Elfs.Abstract;
            // skills                                            Elfs
            character.Stats.Base.Melee          += Statics.Races.Elfs.Melee;
            character.Stats.Base.Arcane         += Statics.Races.Elfs.Arcane;
            character.Stats.Base.Psionics       += Statics.Races.Elfs.Psionics;
            character.Stats.Base.Social         += Statics.Races.Elfs.Social;
            character.Stats.Base.Hide           += Statics.Races.Elfs.Hide;
            character.Stats.Base.Survival       += Statics.Races.Elfs.Survival;
            character.Stats.Base.Tactics        += Statics.Races.Elfs.Tactics;
            character.Stats.Base.Aid            += Statics.Races.Elfs.Aid;
            character.Stats.Base.Crafting       += Statics.Races.Elfs.Crafting;
            character.Stats.Base.Perception           += Statics.Races.Elfs.Perception;
            // assets                                            Elfs
            character.Stats.Base.Defense        += Statics.Races.Elfs.Defense;
            character.Stats.Base.Actions        += Statics.Races.Elfs.Actions;
            character.Stats.Base.Hitpoints      += Statics.Races.Elfs.Hitpoints;
            character.Stats.Base.Mana           += Statics.Races.Elfs.Mana;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            // main
            character.Stats.Base.Strength       += Statics.Races.Dwarfs.Strength;
            character.Stats.Base.Constitution   += Statics.Races.Dwarfs.Constitution;
            character.Stats.Base.Agility        += Statics.Races.Dwarfs.Agility;
            character.Stats.Base.Willpower      += Statics.Races.Dwarfs.Willpower;
            character.Stats.Base.Abstract       += Statics.Races.Dwarfs.Abstract;
            // skills                                            Dwarfs
            character.Stats.Base.Melee          += Statics.Races.Dwarfs.Melee;
            character.Stats.Base.Arcane         += Statics.Races.Dwarfs.Arcane;
            character.Stats.Base.Psionics       += Statics.Races.Dwarfs.Psionics;
            character.Stats.Base.Social         += Statics.Races.Dwarfs.Social;
            character.Stats.Base.Hide           += Statics.Races.Dwarfs.Hide;
            character.Stats.Base.Survival       += Statics.Races.Dwarfs.Survival;
            character.Stats.Base.Tactics        += Statics.Races.Dwarfs.Tactics;
            character.Stats.Base.Aid            += Statics.Races.Dwarfs.Aid;
            character.Stats.Base.Crafting       += Statics.Races.Dwarfs.Crafting;
            character.Stats.Base.Perception           += Statics.Races.Dwarfs.Perception;
            // assets                                            Dwarfs
            character.Stats.Base.Defense        += Statics.Races.Dwarfs.Defense;
            character.Stats.Base.Actions        += Statics.Races.Dwarfs.Actions;
            character.Stats.Base.Hitpoints      += Statics.Races.Dwarfs.Hitpoints;
            character.Stats.Base.Mana           += Statics.Races.Dwarfs.Mana;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            // main
            character.Stats.Base.Strength       += Statics.Cultures.Danarians.Strength;
            character.Stats.Base.Constitution   += Statics.Cultures.Danarians.Constitution;
            character.Stats.Base.Agility        += Statics.Cultures.Danarians.Agility;
            character.Stats.Base.Willpower      += Statics.Cultures.Danarians.Willpower;
            character.Stats.Base.Abstract       += Statics.Cultures.Danarians.Abstract;
            // skills                                      Cultures Danarians
            character.Stats.Base.Melee          += Statics.Cultures.Danarians.Melee;
            character.Stats.Base.Arcane         += Statics.Cultures.Danarians.Arcane;
            character.Stats.Base.Psionics       += Statics.Cultures.Danarians.Psionics;
            character.Stats.Base.Social         += Statics.Cultures.Danarians.Social;
            character.Stats.Base.Hide           += Statics.Cultures.Danarians.Hide;
            character.Stats.Base.Survival       += Statics.Cultures.Danarians.Survival;
            character.Stats.Base.Tactics        += Statics.Cultures.Danarians.Tactics;
            character.Stats.Base.Aid            += Statics.Cultures.Danarians.Aid;
            character.Stats.Base.Crafting       += Statics.Cultures.Danarians.Crafting;
            character.Stats.Base.Perception           += Statics.Cultures.Danarians.Perception;
            // assets                                      Cultures Danarians
            character.Stats.Base.Defense        += Statics.Cultures.Danarians.Defense;
            character.Stats.Base.Actions        += Statics.Cultures.Danarians.Actions;
            character.Stats.Base.Hitpoints      += Statics.Cultures.Danarians.Hitpoints;
            character.Stats.Base.Mana           += Statics.Cultures.Danarians.Mana;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            // main
            character.Stats.Base.Strength       += Statics.Cultures.Highborns.Strength;
            character.Stats.Base.Constitution   += Statics.Cultures.Highborns.Constitution;
            character.Stats.Base.Agility        += Statics.Cultures.Highborns.Agility;
            character.Stats.Base.Willpower      += Statics.Cultures.Highborns.Willpower;
            character.Stats.Base.Abstract       += Statics.Cultures.Highborns.Abstract;
            // skills                                      Cultures Highborns
            character.Stats.Base.Melee          += Statics.Cultures.Highborns.Melee;
            character.Stats.Base.Arcane         += Statics.Cultures.Highborns.Arcane;
            character.Stats.Base.Psionics       += Statics.Cultures.Highborns.Psionics;
            character.Stats.Base.Social         += Statics.Cultures.Highborns.Social;
            character.Stats.Base.Hide           += Statics.Cultures.Highborns.Hide;
            character.Stats.Base.Survival       += Statics.Cultures.Highborns.Survival;
            character.Stats.Base.Tactics        += Statics.Cultures.Highborns.Tactics;
            character.Stats.Base.Aid            += Statics.Cultures.Highborns.Aid;
            character.Stats.Base.Crafting       += Statics.Cultures.Highborns.Crafting;
            character.Stats.Base.Perception           += Statics.Cultures.Highborns.Perception;
            // assets                                      Cultures Highborns
            character.Stats.Base.Defense        += Statics.Cultures.Highborns.Defense;
            character.Stats.Base.Actions        += Statics.Cultures.Highborns.Actions;
            character.Stats.Base.Hitpoints      += Statics.Cultures.Highborns.Hitpoints;
            character.Stats.Base.Mana           += Statics.Cultures.Highborns.Mana;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            // main
            character.Stats.Base.Strength       += Statics.Cultures.Undermountains.Strength;
            character.Stats.Base.Constitution   += Statics.Cultures.Undermountains.Constitution;
            character.Stats.Base.Agility        += Statics.Cultures.Undermountains.Agility;
            character.Stats.Base.Willpower      += Statics.Cultures.Undermountains.Willpower;
            character.Stats.Base.Abstract       += Statics.Cultures.Undermountains.Abstract;
            // skills                                      Cultures Undermountains
            character.Stats.Base.Melee          += Statics.Cultures.Undermountains.Melee;
            character.Stats.Base.Arcane         += Statics.Cultures.Undermountains.Arcane;
            character.Stats.Base.Psionics       += Statics.Cultures.Undermountains.Psionics;
            character.Stats.Base.Social         += Statics.Cultures.Undermountains.Social;
            character.Stats.Base.Hide           += Statics.Cultures.Undermountains.Hide;
            character.Stats.Base.Survival       += Statics.Cultures.Undermountains.Survival;
            character.Stats.Base.Tactics        += Statics.Cultures.Undermountains.Tactics;
            character.Stats.Base.Aid            += Statics.Cultures.Undermountains.Aid;
            character.Stats.Base.Crafting       += Statics.Cultures.Undermountains.Crafting;
            character.Stats.Base.Perception           += Statics.Cultures.Undermountains.Perception;
            // assets                                      Cultures Undermountains
            character.Stats.Base.Defense        += Statics.Cultures.Undermountains.Defense;
            character.Stats.Base.Actions        += Statics.Cultures.Undermountains.Actions;
            character.Stats.Base.Hitpoints      += Statics.Cultures.Undermountains.Hitpoints;
            character.Stats.Base.Mana           += Statics.Cultures.Undermountains.Mana;
        }
        else
        {
            throw new NotImplementedException();
        }

        // specs
        if (create.Spec == Statics.Specs.Warring)
        {
            // main
            character.Stats.Base.Strength       += Statics.Specs.Warrings.Strength;
            character.Stats.Base.Constitution   += Statics.Specs.Warrings.Constitution;
            character.Stats.Base.Agility        += Statics.Specs.Warrings.Agility;
            character.Stats.Base.Willpower      += Statics.Specs.Warrings.Willpower;
            character.Stats.Base.Abstract       += Statics.Specs.Warrings.Abstract;
            // skills                                      Specs.Warrings
            character.Stats.Base.Melee          += Statics.Specs.Warrings.Melee;
            character.Stats.Base.Arcane         += Statics.Specs.Warrings.Arcane;
            character.Stats.Base.Psionics       += Statics.Specs.Warrings.Psionics;
            character.Stats.Base.Social         += Statics.Specs.Warrings.Social;
            character.Stats.Base.Hide           += Statics.Specs.Warrings.Hide;
            character.Stats.Base.Survival       += Statics.Specs.Warrings.Survival;
            character.Stats.Base.Tactics        += Statics.Specs.Warrings.Tactics;
            character.Stats.Base.Aid            += Statics.Specs.Warrings.Aid;
            character.Stats.Base.Crafting       += Statics.Specs.Warrings.Crafting;
            character.Stats.Base.Perception           += Statics.Specs.Warrings.Perception;
            // assets                                      Specs.Warrings
            character.Stats.Base.Defense        += Statics.Specs.Warrings.Defense;
            character.Stats.Base.Actions        += Statics.Specs.Warrings.Actions;
            character.Stats.Base.Hitpoints      += Statics.Specs.Warrings.Hitpoints;
            character.Stats.Base.Mana           += Statics.Specs.Warrings.Mana;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            // main
            character.Stats.Base.Strength       += Statics.Specs.Sorcerys.Strength;
            character.Stats.Base.Constitution   += Statics.Specs.Sorcerys.Constitution;
            character.Stats.Base.Agility        += Statics.Specs.Sorcerys.Agility;
            character.Stats.Base.Willpower      += Statics.Specs.Sorcerys.Willpower;
            character.Stats.Base.Abstract       += Statics.Specs.Sorcerys.Abstract;
            // skills                                      Specs.Sorcerys
            character.Stats.Base.Melee          += Statics.Specs.Sorcerys.Melee;
            character.Stats.Base.Arcane         += Statics.Specs.Sorcerys.Arcane;
            character.Stats.Base.Psionics       += Statics.Specs.Sorcerys.Psionics;
            character.Stats.Base.Social         += Statics.Specs.Sorcerys.Social;
            character.Stats.Base.Hide           += Statics.Specs.Sorcerys.Hide;
            character.Stats.Base.Survival       += Statics.Specs.Sorcerys.Survival;
            character.Stats.Base.Tactics        += Statics.Specs.Sorcerys.Tactics;
            character.Stats.Base.Aid            += Statics.Specs.Sorcerys.Aid;
            character.Stats.Base.Crafting       += Statics.Specs.Sorcerys.Crafting;
            character.Stats.Base.Perception           += Statics.Specs.Sorcerys.Perception;
            // assets                                      Specs.Sorcerys
            character.Stats.Base.Defense        += Statics.Specs.Sorcerys.Defense;
            character.Stats.Base.Actions        += Statics.Specs.Sorcerys.Actions;
            character.Stats.Base.Hitpoints      += Statics.Specs.Sorcerys.Hitpoints;
            character.Stats.Base.Mana           += Statics.Specs.Sorcerys.Mana;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            // main
            character.Stats.Base.Strength       += Statics.Specs.Trackings.Strength;
            character.Stats.Base.Constitution   += Statics.Specs.Trackings.Constitution;
            character.Stats.Base.Agility        += Statics.Specs.Trackings.Agility;
            character.Stats.Base.Willpower      += Statics.Specs.Trackings.Willpower;
            character.Stats.Base.Abstract       += Statics.Specs.Trackings.Abstract;
            // skills                                      Specs.Trackings
            character.Stats.Base.Melee          += Statics.Specs.Trackings.Melee;
            character.Stats.Base.Arcane         += Statics.Specs.Trackings.Arcane;
            character.Stats.Base.Psionics       += Statics.Specs.Trackings.Psionics;
            character.Stats.Base.Social         += Statics.Specs.Trackings.Social;
            character.Stats.Base.Hide           += Statics.Specs.Trackings.Hide;
            character.Stats.Base.Survival       += Statics.Specs.Trackings.Survival;
            character.Stats.Base.Tactics        += Statics.Specs.Trackings.Tactics;
            character.Stats.Base.Aid            += Statics.Specs.Trackings.Aid;
            character.Stats.Base.Crafting       += Statics.Specs.Trackings.Crafting;
            character.Stats.Base.Perception           += Statics.Specs.Trackings.Perception;
            // assets                                      Specs.Trackings
            character.Stats.Base.Defense        += Statics.Specs.Trackings.Defense;
            character.Stats.Base.Actions        += Statics.Specs.Trackings.Actions;
            character.Stats.Base.Hitpoints      += Statics.Specs.Trackings.Hitpoints;
            character.Stats.Base.Mana           += Statics.Specs.Trackings.Mana;
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
        character.Details.BoardId = Guid.Empty;
        character.Details.BoardType = string.Empty;
        character.Details.Renown = 0;
    }

    private void SetInventory(Character character)
    {
        character.Inventory.Add(_items.GenerateSpecificItem(Statics.Items.Types.Weapon));

        var trinket = (Trinket)_items.GenerateSpecificItem(Statics.Items.Types.Trinket);
        character.Regalia.Add(trinket);
    }

    private static void SetWorth(Character character)
    {
        character.Details.Worth =
            (character.Stats.Base.Strength +
            character.Stats.Base.Constitution +
            character.Stats.Base.Agility +
            character.Stats.Base.Willpower +
            character.Stats.Base.Abstract +
            character.Stats.Base.Melee +
            character.Stats.Base.Arcane +
            character.Stats.Base.Psionics +
            character.Stats.Base.Social +
            character.Stats.Base.Hide +
            character.Stats.Base.Survival +
            character.Stats.Base.Tactics +
            character.Stats.Base.Aid +
            character.Stats.Base.Crafting +
            character.Stats.Base.Perception +
            character.Stats.Base.Defense +
            character.Stats.Base.Actions +
            character.Stats.Base.Hitpoints +
            character.Stats.Base.Mana)/10 +
            (int)(character.Details.Wealth * 0.1);
    }

    #endregion
}
