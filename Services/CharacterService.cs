using Models;

namespace Services;

public interface ICharacterService
{
    Character GetCharacterByPlayerId(Guid characterId, Guid playerId);
    Character GetCharacterByPlayerId(CharacterIdentity identity);
    Characters GetAllCharactersByPlayerId(Guid playerId);

    CharacterVm CharacterToCharacterVm(Character character);

    void CreateCharacter(CreateCharacter create);
    void DeleteCharacter(CharacterIdentity identity);

    void EquipItem(EquipItem equipItem);
    void UnequipItem(EquipItem equipItem);
    void SellItem(EquipItem equipItem);
    void Levelup(CharacterLevelup levelup);
    void BuyItemFromTown(EquipItem equipItem);

    CharacterStats ExtractCharacterFightsFromActuals(CharacterStats actuals);
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

    public CharacterStats ExtractCharacterFightsFromActuals(CharacterStats actuals)
    {
        return new CharacterStats
        {
            Strength = actuals.Strength,
            Constitution = actuals.Constitution,
            Agility = actuals.Agility,
            Willpower = actuals.Willpower,
            Abstract = actuals.Abstract,
            Melee = actuals.Melee,
            Arcane = actuals.Arcane,
            Psionics = actuals.Psionics,
            Social = actuals.Social,
            Hide = actuals.Hide,
            Survival = actuals.Survival,
            Tactics = actuals.Tactics,
            Aid = actuals.Aid,
            Crafting = actuals.Crafting,
            Perception = actuals.Perception,
            Defense = actuals.Defense,
            Actions = actuals.Actions,
            Endurance = actuals.Endurance,
            Accretion = actuals.Accretion,
        };
    }

    public void CreateCharacter(CreateCharacter create)
    {
        _validator.ValidateOnCharacterCreate(create);

        var character = new Character
        {
            Identity = new CharacterIdentity
            {
                CharacterId = Guid.NewGuid(),
                PlayerId = create.PlayerId,
            },
        };

        SetDetails(create, character);
        SetStats(create, character);
        SetInventory(character);
        SetWorth(character);
        SetActuals(character);

        _snapshot.Characters.Add(character);
    }

    public void DeleteCharacter(CharacterIdentity identity)
    {
        var character = _validator.ValidateOnCharacterDelete(identity)!;

        _snapshot.Characters.Remove(character);
    }

    public Character GetCharacterByPlayerId(CharacterIdentity identity)
    {
        _validator.ValidateAgainstNull(identity);

        return GetCharacterByPlayerId(identity.CharacterId, identity.PlayerId);
    }

    public Character GetCharacterByPlayerId(Guid characterId, Guid playerId)
    {
        _validator.ValidatePlayerExists(playerId);
        _validator.ValidateCharacterPlayerCombination(characterId, playerId);
        var character = _validator.ValidateCharacterExists(characterId);
        SetActuals(character);
        SetWorth(character);

        return character;
    }

    public CharacterVm CharacterToCharacterVm(Character character)
    {
        return new CharacterVm
        {
            CharacterId = character.Identity.CharacterId,
            Roll = 0,
            Details = character.Details,
            Stats = new Stats
            {
                Base = character.Stats.Actuals,
                Fights = character.Stats.Actuals,
            }
        };
    }

    public Characters GetAllCharactersByPlayerId(Guid playerId)
    {
        _validator.ValidatePlayerExists(playerId);

        var listOfCharacters = new Characters();

        _snapshot.Characters
            .Where(s => s.Identity.PlayerId == playerId).ToList()
            .ForEach(s =>
            {
                listOfCharacters.CharactersList.Add(CharacterToCharacterVm(s));
            });

        return listOfCharacters;
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
            var roll = _dice.RollForCharacter(character, Statics.Stats.Social);

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

    public static CharacterStats GetCharacterFights(Character character)
    {
        return new CharacterStats
        {
            // main
            Strength        = character.Stats.Actuals.Strength,
            Constitution    = character.Stats.Actuals.Constitution,
            Agility         = character.Stats.Actuals.Agility,
            Willpower       = character.Stats.Actuals.Willpower,
            Abstract        = character.Stats.Actuals.Abstract,
            // skills
            Melee           = character.Stats.Actuals.Melee,
            Arcane          = character.Stats.Actuals.Arcane,
            Psionics        = character.Stats.Actuals.Psionics,
            Social          = character.Stats.Actuals.Social,
            Hide            = character.Stats.Actuals.Hide,
            Survival        = character.Stats.Actuals.Survival,
            Tactics         = character.Stats.Actuals.Tactics,
            Aid             = character.Stats.Actuals.Aid,
            Crafting        = character.Stats.Actuals.Crafting,
            Perception      = character.Stats.Actuals.Perception,
            // assets
            Defense         = character.Stats.Actuals.Defense,
            Actions         = character.Stats.Actuals.Actions,
            Endurance       = character.Stats.Actuals.Endurance,
            Accretion       = character.Stats.Actuals.Accretion
        };
    }

    #region private methods
    private static void SetActuals(Character character)
    {
        // stats
        character.Stats.Actuals.Strength        = character.Stats.Base.Strength     + character.Inventory.Select(s => s.Bonuses.Strength).Sum()     + character.Regalia.Select(s => s.Bonuses.Strength).Sum();
        character.Stats.Actuals.Constitution    = character.Stats.Base.Constitution + character.Inventory.Select(s => s.Bonuses.Constitution).Sum() + character.Regalia.Select(s => s.Bonuses.Constitution).Sum();
        character.Stats.Actuals.Agility         = character.Stats.Base.Agility      + character.Inventory.Select(s => s.Bonuses.Agility).Sum()      + character.Regalia.Select(s => s.Bonuses.Agility).Sum();
        character.Stats.Actuals.Willpower       = character.Stats.Base.Willpower    + character.Inventory.Select(s => s.Bonuses.Willpower).Sum()    + character.Regalia.Select(s => s.Bonuses.Willpower).Sum();
        character.Stats.Actuals.Abstract        = character.Stats.Base.Abstract     + character.Inventory.Select(s => s.Bonuses.Abstract).Sum()     + character.Regalia.Select(s => s.Bonuses.Abstract).Sum();
        // skills
        character.Stats.Actuals.Melee           = character.Stats.Base.Melee        + character.Inventory.Select(s => s.Bonuses.Melee).Sum()        + character.Regalia.Select(s => s.Bonuses.Melee).Sum();
        character.Stats.Actuals.Arcane          = character.Stats.Base.Arcane       + character.Inventory.Select(s => s.Bonuses.Arcane).Sum()       + character.Regalia.Select(s => s.Bonuses.Arcane).Sum();
        character.Stats.Actuals.Psionics        = character.Stats.Base.Psionics     + character.Inventory.Select(s => s.Bonuses.Psionics).Sum()     + character.Regalia.Select(s => s.Bonuses.Psionics).Sum();
        character.Stats.Actuals.Social          = character.Stats.Base.Social       + character.Inventory.Select(s => s.Bonuses.Social).Sum()       + character.Regalia.Select(s => s.Bonuses.Social).Sum();
        character.Stats.Actuals.Hide            = character.Stats.Base.Hide         + character.Inventory.Select(s => s.Bonuses.Hide).Sum()         + character.Regalia.Select(s => s.Bonuses.Hide).Sum();
        character.Stats.Actuals.Survival        = character.Stats.Base.Survival     + character.Inventory.Select(s => s.Bonuses.Survival).Sum()     + character.Regalia.Select(s => s.Bonuses.Survival).Sum();
        character.Stats.Actuals.Tactics         = character.Stats.Base.Tactics      + character.Inventory.Select(s => s.Bonuses.Tactics).Sum()      + character.Regalia.Select(s => s.Bonuses.Tactics).Sum();
        character.Stats.Actuals.Aid             = character.Stats.Base.Aid          + character.Inventory.Select(s => s.Bonuses.Aid).Sum()          + character.Regalia.Select(s => s.Bonuses.Aid).Sum();
        character.Stats.Actuals.Crafting        = character.Stats.Base.Crafting     + character.Inventory.Select(s => s.Bonuses.Crafting).Sum()     + character.Regalia.Select(s => s.Bonuses.Crafting).Sum();
        character.Stats.Actuals.Perception      = character.Stats.Base.Perception   + character.Inventory.Select(s => s.Bonuses.Perception).Sum()   + character.Regalia.Select(s => s.Bonuses.Perception).Sum();
        // assets
        character.Stats.Actuals.Defense         = character.Stats.Base.Defense      + character.Inventory.Select(s => s.Bonuses.Defense).Sum()      + character.Regalia.Select(s => s.Bonuses.Defense).Sum();
        character.Stats.Actuals.Actions         = character.Stats.Base.Actions      + character.Inventory.Select(s => s.Bonuses.Actions).Sum()      + character.Regalia.Select(s => s.Bonuses.Actions).Sum();
        character.Stats.Actuals.Endurance       = character.Stats.Base.Endurance    + character.Inventory.Select(s => s.Bonuses.Endurance).Sum()    + character.Regalia.Select(s => s.Bonuses.Endurance).Sum();
        character.Stats.Actuals.Accretion       = character.Stats.Base.Accretion    + character.Inventory.Select(s => s.Bonuses.Accretion).Sum()    + character.Regalia.Select(s => s.Bonuses.Accretion).Sum();
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
            character.Stats.Base.Perception     += Statics.Races.Humans.Perception;
            // assets                                            Humans
            character.Stats.Base.Defense        += Statics.Races.Humans.Defense;
            character.Stats.Base.Actions        += Statics.Races.Humans.Actions;
            character.Stats.Base.Endurance      += Statics.Races.Humans.Endurance;
            character.Stats.Base.Accretion      += Statics.Races.Humans.Accretion;
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
            character.Stats.Base.Perception     += Statics.Races.Elfs.Perception;
            // assets                                            Elfs
            character.Stats.Base.Defense        += Statics.Races.Elfs.Defense;
            character.Stats.Base.Actions        += Statics.Races.Elfs.Actions;
            character.Stats.Base.Endurance      += Statics.Races.Elfs.Endurance;
            character.Stats.Base.Accretion      += Statics.Races.Elfs.Accretion;
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
            character.Stats.Base.Perception     += Statics.Races.Dwarfs.Perception;
            // assets                                            Dwarfs
            character.Stats.Base.Defense        += Statics.Races.Dwarfs.Defense;
            character.Stats.Base.Actions        += Statics.Races.Dwarfs.Actions;
            character.Stats.Base.Endurance      += Statics.Races.Dwarfs.Endurance;
            character.Stats.Base.Accretion      += Statics.Races.Dwarfs.Accretion;
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
            character.Stats.Base.Perception     += Statics.Cultures.Danarians.Perception;
            // assets                                      Cultures Danarians
            character.Stats.Base.Defense        += Statics.Cultures.Danarians.Defense;
            character.Stats.Base.Actions        += Statics.Cultures.Danarians.Actions;
            character.Stats.Base.Endurance      += Statics.Cultures.Danarians.Endurance;
            character.Stats.Base.Accretion      += Statics.Cultures.Danarians.Accretion;
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
            character.Stats.Base.Perception     += Statics.Cultures.Highborns.Perception;
            // assets                                      Cultures Highborns
            character.Stats.Base.Defense        += Statics.Cultures.Highborns.Defense;
            character.Stats.Base.Actions        += Statics.Cultures.Highborns.Actions;
            character.Stats.Base.Endurance      += Statics.Cultures.Highborns.Endurance;
            character.Stats.Base.Accretion      += Statics.Cultures.Highborns.Accretion;
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
            character.Stats.Base.Perception     += Statics.Cultures.Undermountains.Perception;
            // assets                                      Cultures Undermountains
            character.Stats.Base.Defense        += Statics.Cultures.Undermountains.Defense;
            character.Stats.Base.Actions        += Statics.Cultures.Undermountains.Actions;
            character.Stats.Base.Endurance      += Statics.Cultures.Undermountains.Endurance;
            character.Stats.Base.Accretion      += Statics.Cultures.Undermountains.Accretion;
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
            character.Stats.Base.Perception     += Statics.Specs.Warrings.Perception;
            // assets                                      Specs.Warrings
            character.Stats.Base.Defense        += Statics.Specs.Warrings.Defense;
            character.Stats.Base.Actions        += Statics.Specs.Warrings.Actions;
            character.Stats.Base.Endurance      += Statics.Specs.Warrings.Endurance;
            character.Stats.Base.Accretion      += Statics.Specs.Warrings.Accretion;
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
            character.Stats.Base.Perception     += Statics.Specs.Sorcerys.Perception;
            // assets                                      Specs.Sorcerys
            character.Stats.Base.Defense        += Statics.Specs.Sorcerys.Defense;
            character.Stats.Base.Actions        += Statics.Specs.Sorcerys.Actions;
            character.Stats.Base.Endurance      += Statics.Specs.Sorcerys.Endurance;
            character.Stats.Base.Accretion      += Statics.Specs.Sorcerys.Accretion;
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
            character.Stats.Base.Perception     += Statics.Specs.Trackings.Perception;
            // assets                                      Specs.Trackings
            character.Stats.Base.Defense        += Statics.Specs.Trackings.Defense;
            character.Stats.Base.Actions        += Statics.Specs.Trackings.Actions;
            character.Stats.Base.Endurance      += Statics.Specs.Trackings.Endurance;
            character.Stats.Base.Accretion      += Statics.Specs.Trackings.Accretion;
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
        character.Details.EntityLevel = 1;
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
            character.Stats.Base.Endurance +
            character.Stats.Base.Accretion)/10 +
            (int)(character.Details.Wealth * 0.1);
    }

    #endregion
}
