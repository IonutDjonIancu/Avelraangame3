using Models;
using Newtonsoft.Json;

namespace Services;

public interface ICharacterService
{
    /// <summary>
    /// Includes character actuals.
    /// </summary>
    /// <param name="identity"></param>
    /// <returns></returns>
    Character GetCharacter(CharacterIdentity identity);
    Characters GetAllAliveCharacters();
    Characters GetAllLockedCharacters();
    Characters GetAllDuelistCharacters();

    string CreateCharacter(CreateCharacter create);
    string ExportCharacter(CharacterIdentity identity);
    ImportCharacterResponse ImportCharacter(ImportCharacter import);

    Character EquipItem(EquipItem equipItem);
    Character UnequipItem(EquipItem equipItem);
    Character SellItem(EquipItem equipItem);
    Character BuyItem(EquipItem equipItem);
    Character Levelup(CharacterLevelup levelup);

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

    public string CreateCharacter(CreateCharacter create)
    {
        // validate player
        _validator.ValidateOnCreateCharacter(create);

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
        SetInventory(character);
        SetWorth(character);

        _snapshot.Players.FirstOrDefault(s => s.Name == create.PlayerName)!.Characters.Add(character);

        var characterString = EncryptionService.EncryptString(JsonConvert.SerializeObject(character));

        return characterString;
    }

    public Characters GetAllAliveCharacters(string playerName)
    {
        // validate

        var listOfCharacters = new Characters();

        _snapshot.Players.FirstOrDefault(s => s.Name == playerName)!.Characters.ForEach(s =>
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

    public Characters GetAllLockedCharacters()
    {
        return new Characters
        {
            CharactersPortraits = _snapshot.Characters
                .Where(s => s.Details.IsAlive 
                            && !s.Details.IsNpc 
                            && s.Details.IsLocked)
                .Select(s => s.Details.Portrait)
                .ToList(),
        };
    }

    public Characters GetAllDuelistCharacters()
    {
        return new Characters
        {
            CharactersPortraits = _snapshot.Characters
                .Where(s => s.Details.IsAlive
                            && !s.Details.IsNpc
                            && s.Details.IsLocked
                            && s.Details.BoardType == Statics.Boards.Types.Duel)
                .Select(s => s.Details.Portrait)
                .ToList(),
        };
    }

    public Character GetCharacter(CharacterIdentity identity)
    {
        var character = _validator.ValidateOnGetCharacter(identity);

        CalculateActuals(character);
        SetWorth(character);

        if (character.Actuals.Defense > 90)
            character.Actuals.Defense = 90; // capped at 90 %

        if (character.Actuals.Resist > 100)
            character.Actuals.Resist = 100; // capped at 100 %

        if (character.Fights.Defense > 90)
            character.Fights.Defense = 90; // capped at 90 %

        if (character.Fights.Resist > 100)
            character.Fights.Resist = 100; // capped at 100 %

        return character;
    }

    public string ExportCharacter(CharacterIdentity identity)
    {
        var character = _validator.ValidateOnExportCharacter(identity);

        return EncryptionService.EncryptString(JsonConvert.SerializeObject(character));
    }

    public ImportCharacterResponse ImportCharacter(ImportCharacter import)
    {
        _validator.ValidateOnImportCharacter(import);   

        var decryptString = EncryptionService.DecryptString(import.CharacterString);

        var character = JsonConvert.DeserializeObject<Character>(decryptString)!;

        if (_snapshot.Characters.Any(s => s.Identity.Id == character.Identity.Id))
        {
            _snapshot.Characters.RemoveWhere(s => s.Identity.Id == character.Identity.Id);
        }

        _snapshot.Characters.Add(character);

        return new ImportCharacterResponse
        {
            CharacterIdentity = character.Identity
        };
    }

    public Character EquipItem(EquipItem equipItem)
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

        return GetCharacter(character.Identity);
    }

    public Character UnequipItem(EquipItem equipItem)
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

        return GetCharacter(character.Identity);
    }

    public Character SellItem(EquipItem equipItem)
    {
        var (item, character) = _validator.ValidateSellItem(equipItem);

        if (_snapshot.ItemsSold.Count >= 10000)
            _snapshot.ItemsSold.Remove(_snapshot.ItemsSold.First());

        _snapshot.ItemsSold.Add(item.Id);

        var (isRollVsEffortSuccess, _) = _dice.RollVsEffort(character, Statics.Stats.Social, _dice.Rolld20NoReroll(), true, false);

        character.Details.Wealth += isRollVsEffortSuccess ? item.Value + (int)(item.Value * 0.25) : item.Value;
        
        if (item.Type == Statics.Items.Types.Trinket)
        {
            character.Supplies.Regalia.Remove(item as Trinket);
        }
        else
        {
            character.Supplies.Items.Remove(item);
        }

        return GetCharacter(character.Identity);
    }

    public Character BuyItem(EquipItem equipItem)
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

            _snapshot.Market.Remove(item);

            var (isRollVsEffortSuccess, _) = _dice.RollVsEffort(character, Statics.Stats.Social, _dice.Rolld20NoReroll(), true, false);

            character.Details.Wealth -= isRollVsEffortSuccess ? item.Value - (int)(item.Value * 0.25) : item.Value;

            return GetCharacter(character.Identity);
        }
    }

    public Character Levelup(CharacterLevelup levelup)
    {
        var character = _validator.ValidateLevelupAndReturn(levelup);

        var stat = typeof(CharacterStats).GetProperty(levelup.Stat)!;
        var value = (int)stat.GetValue(character.Stats)!;
        int valueToAdd;

        if (value <= 0)
        {
            valueToAdd = 1;
        }
        else
        {
            valueToAdd = value + 1;
        }

        stat.SetValue(character.Stats, valueToAdd);

        character.Details.Levelup -= valueToAdd;

        return GetCharacter(character.Identity);
    }

    public void SetCharacterFights(Character character)
    {
        // main
        character.Fights.Strength = character.Actuals.Strength;
        character.Fights.Constitution = character.Actuals.Constitution;
        character.Fights.Agility = character.Actuals.Agility;
        character.Fights.Willpower = character.Actuals.Willpower;
        character.Fights.Abstract = character.Actuals.Abstract;
        // skills
        character.Fights.Melee = character.Actuals.Melee;
        character.Fights.Arcane = character.Actuals.Arcane;
        character.Fights.Psionics = character.Actuals.Psionics;
        character.Fights.Social = character.Actuals.Social;
        character.Fights.Hide = character.Actuals.Hide;
        character.Fights.Survival = character.Actuals.Survival;
        character.Fights.Tactics = character.Actuals.Tactics;
        character.Fights.Aid = character.Actuals.Aid;
        character.Fights.Crafting = character.Actuals.Crafting;
        character.Fights.Spot = character.Actuals.Spot;
        // assets
        character.Fights.Defense = character.Actuals.Defense;
        character.Fights.Resist = character.Actuals.Resist;
        character.Fights.Actions = character.Actuals.Actions;
        character.Fights.Endurance = character.Actuals.Endurance;
        character.Fights.Accretion = character.Actuals.Accretion;
    }

    #region private methods
    private static void CalculateActuals(Character character)
    {
        // stats
        character.Actuals.Strength      = character.Stats.Strength      + character.Inventory.Select(s => s.Stats.Strength).Sum()       + character.Regalia.Select(s => s.Stats.Strength).Sum();
        character.Actuals.Constitution  = character.Stats.Constitution  + character.Inventory.Select(s => s.Stats.Constitution).Sum()   + character.Regalia.Select(s => s.Stats.Constitution).Sum();
        character.Actuals.Agility       = character.Stats.Agility       + character.Inventory.Select(s => s.Stats.Agility).Sum()        + character.Regalia.Select(s => s.Stats.Agility).Sum();
        character.Actuals.Willpower     = character.Stats.Willpower     + character.Inventory.Select(s => s.Stats.Willpower).Sum()      + character.Regalia.Select(s => s.Stats.Willpower).Sum();
        character.Actuals.Abstract      = character.Stats.Abstract      + character.Inventory.Select(s => s.Stats.Abstract).Sum()       + character.Regalia.Select(s => s.Stats.Abstract).Sum();
        // skills
        character.Actuals.Melee         = character.Stats.Melee         + character.Inventory.Select(s => s.Stats.Melee).Sum()          + character.Regalia.Select(s => s.Stats.Melee).Sum();
        character.Actuals.Arcane        = character.Stats.Arcane        + character.Inventory.Select(s => s.Stats.Arcane).Sum()         + character.Regalia.Select(s => s.Stats.Arcane).Sum();
        character.Actuals.Psionics      = character.Stats.Psionics      + character.Inventory.Select(s => s.Stats.Psionics).Sum()       + character.Regalia.Select(s => s.Stats.Psionics).Sum();
        character.Actuals.Social        = character.Stats.Social        + character.Inventory.Select(s => s.Stats.Social).Sum()         + character.Regalia.Select(s => s.Stats.Social).Sum();
        character.Actuals.Hide          = character.Stats.Hide          + character.Inventory.Select(s => s.Stats.Hide).Sum()           + character.Regalia.Select(s => s.Stats.Hide).Sum();
        character.Actuals.Survival      = character.Stats.Survival      + character.Inventory.Select(s => s.Stats.Survival).Sum()       + character.Regalia.Select(s => s.Stats.Survival).Sum();
        character.Actuals.Tactics       = character.Stats.Tactics       + character.Inventory.Select(s => s.Stats.Tactics).Sum()        + character.Regalia.Select(s => s.Stats.Tactics).Sum();
        character.Actuals.Aid           = character.Stats.Aid           + character.Inventory.Select(s => s.Stats.Aid).Sum()            + character.Regalia.Select(s => s.Stats.Aid).Sum();
        character.Actuals.Crafting      = character.Stats.Crafting      + character.Inventory.Select(s => s.Stats.Crafting).Sum()       + character.Regalia.Select(s => s.Stats.Crafting).Sum();
        character.Actuals.Spot    = character.Stats.Spot    + character.Inventory.Select(s => s.Stats.Spot).Sum()     + character.Regalia.Select(s => s.Stats.Spot).Sum();
        // assets
        character.Actuals.Defense       = character.Stats.Defense       + character.Inventory.Select(s => s.Stats.Defense).Sum()        + character.Regalia.Select(s => s.Stats.Defense).Sum();
        character.Actuals.Resist        = character.Stats.Resist        + character.Inventory.Select(s => s.Stats.Resist ).Sum()        + character.Regalia.Select(s => s.Stats.Resist).Sum();
        character.Actuals.Actions       = character.Stats.Actions       + character.Inventory.Select(s => s.Stats.Actions).Sum()        + character.Regalia.Select(s => s.Stats.Actions).Sum();
        character.Actuals.Endurance     = character.Stats.Endurance     + character.Inventory.Select(s => s.Stats.Endurance).Sum()      + character.Regalia.Select(s => s.Stats.Endurance).Sum();
        character.Actuals.Accretion     = character.Stats.Accretion     + character.Inventory.Select(s => s.Stats.Accretion).Sum()      + character.Regalia.Select(s => s.Stats.Accretion).Sum();
    }

    private static void SetStats(CreateCharacter create, Character character)
    {
        // races
        if (create.Race == Statics.Races.Human)
        {
            // main
            character.Stats.Strength        += Statics.Races.Humans.Strength;
            character.Stats.Constitution    += Statics.Races.Humans.Constitution;
            character.Stats.Agility         += Statics.Races.Humans.Agility;
            character.Stats.Willpower       += Statics.Races.Humans.Willpower;
            character.Stats.Abstract        += Statics.Races.Humans.Abstract;
            // skills                                        Humans
            character.Stats.Melee           += Statics.Races.Humans.Melee;
            character.Stats.Arcane          += Statics.Races.Humans.Arcane;
            character.Stats.Psionics        += Statics.Races.Humans.Psionics;
            character.Stats.Social          += Statics.Races.Humans.Social;
            character.Stats.Hide            += Statics.Races.Humans.Hide;
            character.Stats.Survival        += Statics.Races.Humans.Survival;
            character.Stats.Tactics         += Statics.Races.Humans.Tactics;
            character.Stats.Aid             += Statics.Races.Humans.Aid;
            character.Stats.Crafting        += Statics.Races.Humans.Crafting;
            character.Stats.Spot      += Statics.Races.Humans.Perception;
            // assets                                        Humans
            character.Stats.Defense         += Statics.Races.Humans.Defense;
            character.Stats.Resist          += Statics.Races.Humans.Resist;
            character.Stats.Actions         += Statics.Races.Humans.Actions;
            character.Stats.Endurance       += Statics.Races.Humans.Endurance;
            character.Stats.Accretion       += Statics.Races.Humans.Accretion;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            // main
            character.Stats.Strength        += Statics.Races.Elfs.Strength;
            character.Stats.Constitution    += Statics.Races.Elfs.Constitution;
            character.Stats.Agility         += Statics.Races.Elfs.Agility;
            character.Stats.Willpower       += Statics.Races.Elfs.Willpower;
            character.Stats.Abstract        += Statics.Races.Elfs.Abstract;
            // skills                                        Elfs
            character.Stats.Melee           += Statics.Races.Elfs.Melee;
            character.Stats.Arcane          += Statics.Races.Elfs.Arcane;
            character.Stats.Psionics        += Statics.Races.Elfs.Psionics;
            character.Stats.Social          += Statics.Races.Elfs.Social;
            character.Stats.Hide            += Statics.Races.Elfs.Hide;
            character.Stats.Survival        += Statics.Races.Elfs.Survival;
            character.Stats.Tactics         += Statics.Races.Elfs.Tactics;
            character.Stats.Aid             += Statics.Races.Elfs.Aid;
            character.Stats.Crafting        += Statics.Races.Elfs.Crafting;
            character.Stats.Spot      += Statics.Races.Elfs.Perception;
            // assets                                        Elfs
            character.Stats.Defense         += Statics.Races.Elfs.Defense;
            character.Stats.Resist          += Statics.Races.Elfs.Resist;
            character.Stats.Actions         += Statics.Races.Elfs.Actions;
            character.Stats.Endurance       += Statics.Races.Elfs.Endurance;
            character.Stats.Accretion       += Statics.Races.Elfs.Accretion;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            // main
            character.Stats.Strength        += Statics.Races.Dwarfs.Strength;
            character.Stats.Constitution    += Statics.Races.Dwarfs.Constitution;
            character.Stats.Agility         += Statics.Races.Dwarfs.Agility;
            character.Stats.Willpower       += Statics.Races.Dwarfs.Willpower;
            character.Stats.Abstract        += Statics.Races.Dwarfs.Abstract;
            // skills                                        Dwarfs
            character.Stats.Melee           += Statics.Races.Dwarfs.Melee;
            character.Stats.Arcane          += Statics.Races.Dwarfs.Arcane;
            character.Stats.Psionics        += Statics.Races.Dwarfs.Psionics;
            character.Stats.Social          += Statics.Races.Dwarfs.Social;
            character.Stats.Hide            += Statics.Races.Dwarfs.Hide;
            character.Stats.Survival        += Statics.Races.Dwarfs.Survival;
            character.Stats.Tactics         += Statics.Races.Dwarfs.Tactics;
            character.Stats.Aid             += Statics.Races.Dwarfs.Aid;
            character.Stats.Crafting        += Statics.Races.Dwarfs.Crafting;
            character.Stats.Spot      += Statics.Races.Dwarfs.Perception;
            // assets                                        Dwarfs
            character.Stats.Defense         += Statics.Races.Dwarfs.Defense;
            character.Stats.Resist          += Statics.Races.Dwarfs.Resist;
            character.Stats.Actions         += Statics.Races.Dwarfs.Actions;
            character.Stats.Endurance       += Statics.Races.Dwarfs.Endurance;
            character.Stats.Accretion       += Statics.Races.Dwarfs.Accretion;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            // main
            character.Stats.Strength        += Statics.Cultures.Danarians.Strength;
            character.Stats.Constitution    += Statics.Cultures.Danarians.Constitution;
            character.Stats.Agility         += Statics.Cultures.Danarians.Agility;
            character.Stats.Willpower       += Statics.Cultures.Danarians.Willpower;
            character.Stats.Abstract        += Statics.Cultures.Danarians.Abstract;
            // skills                                  Cultures.Danarians
            character.Stats.Melee           += Statics.Cultures.Danarians.Melee;
            character.Stats.Arcane          += Statics.Cultures.Danarians.Arcane;
            character.Stats.Psionics        += Statics.Cultures.Danarians.Psionics;
            character.Stats.Social          += Statics.Cultures.Danarians.Social;
            character.Stats.Hide            += Statics.Cultures.Danarians.Hide;
            character.Stats.Survival        += Statics.Cultures.Danarians.Survival;
            character.Stats.Tactics         += Statics.Cultures.Danarians.Tactics;
            character.Stats.Aid             += Statics.Cultures.Danarians.Aid;
            character.Stats.Crafting        += Statics.Cultures.Danarians.Crafting;
            character.Stats.Spot      += Statics.Cultures.Danarians.Perception;
            // assets                                  Cultures.Danarians
            character.Stats.Defense         += Statics.Cultures.Danarians.Defense;
            character.Stats.Resist          += Statics.Cultures.Danarians.Resist;
            character.Stats.Actions         += Statics.Cultures.Danarians.Actions;
            character.Stats.Endurance       += Statics.Cultures.Danarians.Endurance;
            character.Stats.Accretion       += Statics.Cultures.Danarians.Accretion;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            // main
            character.Stats.Strength        += Statics.Cultures.Highborns.Strength;
            character.Stats.Constitution    += Statics.Cultures.Highborns.Constitution;
            character.Stats.Agility         += Statics.Cultures.Highborns.Agility;
            character.Stats.Willpower       += Statics.Cultures.Highborns.Willpower;
            character.Stats.Abstract        += Statics.Cultures.Highborns.Abstract;
            // skills                                  Cultures.Highborns
            character.Stats.Melee           += Statics.Cultures.Highborns.Melee;
            character.Stats.Arcane          += Statics.Cultures.Highborns.Arcane;
            character.Stats.Psionics        += Statics.Cultures.Highborns.Psionics;
            character.Stats.Social          += Statics.Cultures.Highborns.Social;
            character.Stats.Hide            += Statics.Cultures.Highborns.Hide;
            character.Stats.Survival        += Statics.Cultures.Highborns.Survival;
            character.Stats.Tactics         += Statics.Cultures.Highborns.Tactics;
            character.Stats.Aid             += Statics.Cultures.Highborns.Aid;
            character.Stats.Crafting        += Statics.Cultures.Highborns.Crafting;
            character.Stats.Spot      += Statics.Cultures.Highborns.Perception;
            // assets                                  Cultures.Highborns
            character.Stats.Defense         += Statics.Cultures.Highborns.Defense;
            character.Stats.Resist          += Statics.Cultures.Highborns.Resist;
            character.Stats.Actions         += Statics.Cultures.Highborns.Actions;
            character.Stats.Endurance       += Statics.Cultures.Highborns.Endurance;
            character.Stats.Accretion       += Statics.Cultures.Highborns.Accretion;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            // main
            character.Stats.Strength        += Statics.Cultures.Undermountains.Strength;
            character.Stats.Constitution    += Statics.Cultures.Undermountains.Constitution;
            character.Stats.Agility         += Statics.Cultures.Undermountains.Agility;
            character.Stats.Willpower       += Statics.Cultures.Undermountains.Willpower;
            character.Stats.Abstract        += Statics.Cultures.Undermountains.Abstract;
            // skills                                  Cultures.Undermountains
            character.Stats.Melee           += Statics.Cultures.Undermountains.Melee;
            character.Stats.Arcane          += Statics.Cultures.Undermountains.Arcane;
            character.Stats.Psionics        += Statics.Cultures.Undermountains.Psionics;
            character.Stats.Social          += Statics.Cultures.Undermountains.Social;
            character.Stats.Hide            += Statics.Cultures.Undermountains.Hide;
            character.Stats.Survival        += Statics.Cultures.Undermountains.Survival;
            character.Stats.Tactics         += Statics.Cultures.Undermountains.Tactics;
            character.Stats.Aid             += Statics.Cultures.Undermountains.Aid;
            character.Stats.Crafting        += Statics.Cultures.Undermountains.Crafting;
            character.Stats.Spot      += Statics.Cultures.Undermountains.Perception;
            // assets                                  Cultures.Undermountains
            character.Stats.Defense         += Statics.Cultures.Undermountains.Defense;
            character.Stats.Resist          += Statics.Cultures.Undermountains.Resist;
            character.Stats.Actions         += Statics.Cultures.Undermountains.Actions;
            character.Stats.Endurance       += Statics.Cultures.Undermountains.Endurance;
            character.Stats.Accretion       += Statics.Cultures.Undermountains.Accretion;
        }
        else
        {
            throw new NotImplementedException();
        }

        // specs
        if (create.Spec == Statics.Specs.Warring)
        {
            // main
            character.Stats.Strength        += Statics.Specs.Warrings.Strength;
            character.Stats.Constitution    += Statics.Specs.Warrings.Constitution;
            character.Stats.Agility         += Statics.Specs.Warrings.Agility;
            character.Stats.Willpower       += Statics.Specs.Warrings.Willpower;
            character.Stats.Abstract        += Statics.Specs.Warrings.Abstract;
            // skills                                  Specs.Warrings
            character.Stats.Melee           += Statics.Specs.Warrings.Melee;
            character.Stats.Arcane          += Statics.Specs.Warrings.Arcane;
            character.Stats.Psionics        += Statics.Specs.Warrings.Psionics;
            character.Stats.Social          += Statics.Specs.Warrings.Social;
            character.Stats.Hide            += Statics.Specs.Warrings.Hide;
            character.Stats.Survival        += Statics.Specs.Warrings.Survival;
            character.Stats.Tactics         += Statics.Specs.Warrings.Tactics;
            character.Stats.Aid             += Statics.Specs.Warrings.Aid;
            character.Stats.Crafting        += Statics.Specs.Warrings.Crafting;
            character.Stats.Spot      += Statics.Specs.Warrings.Perception;
            // assets                                  Specs.Warrings
            character.Stats.Defense         += Statics.Specs.Warrings.Defense;
            character.Stats.Resist          += Statics.Specs.Warrings.Resist;
            character.Stats.Actions         += Statics.Specs.Warrings.Actions;
            character.Stats.Endurance       += Statics.Specs.Warrings.Endurance;
            character.Stats.Accretion       += Statics.Specs.Warrings.Accretion;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            // main
            character.Stats.Strength        += Statics.Specs.Sorcerys.Strength;
            character.Stats.Constitution    += Statics.Specs.Sorcerys.Constitution;
            character.Stats.Agility         += Statics.Specs.Sorcerys.Agility;
            character.Stats.Willpower       += Statics.Specs.Sorcerys.Willpower;
            character.Stats.Abstract        += Statics.Specs.Sorcerys.Abstract;
            // skills                                  Specs.Sorcerys
            character.Stats.Melee           += Statics.Specs.Sorcerys.Melee;
            character.Stats.Arcane          += Statics.Specs.Sorcerys.Arcane;
            character.Stats.Psionics        += Statics.Specs.Sorcerys.Psionics;
            character.Stats.Social          += Statics.Specs.Sorcerys.Social;
            character.Stats.Hide            += Statics.Specs.Sorcerys.Hide;
            character.Stats.Survival        += Statics.Specs.Sorcerys.Survival;
            character.Stats.Tactics         += Statics.Specs.Sorcerys.Tactics;
            character.Stats.Aid             += Statics.Specs.Sorcerys.Aid;
            character.Stats.Crafting        += Statics.Specs.Sorcerys.Crafting;
            character.Stats.Spot      += Statics.Specs.Sorcerys.Perception;
            // assets                                  Specs.Sorcerys
            character.Stats.Defense         += Statics.Specs.Sorcerys.Defense;
            character.Stats.Resist          += Statics.Specs.Sorcerys.Resist;
            character.Stats.Actions         += Statics.Specs.Sorcerys.Actions;
            character.Stats.Endurance       += Statics.Specs.Sorcerys.Endurance;
            character.Stats.Accretion       += Statics.Specs.Sorcerys.Accretion;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            // main
            character.Stats.Strength        += Statics.Specs.Trackings.Strength;
            character.Stats.Constitution    += Statics.Specs.Trackings.Constitution;
            character.Stats.Agility         += Statics.Specs.Trackings.Agility;
            character.Stats.Willpower       += Statics.Specs.Trackings.Willpower;
            character.Stats.Abstract        += Statics.Specs.Trackings.Abstract;
            // skills                                  Specs.Trackings
            character.Stats.Melee           += Statics.Specs.Trackings.Melee;
            character.Stats.Arcane          += Statics.Specs.Trackings.Arcane;
            character.Stats.Psionics        += Statics.Specs.Trackings.Psionics;
            character.Stats.Social          += Statics.Specs.Trackings.Social;
            character.Stats.Hide            += Statics.Specs.Trackings.Hide;
            character.Stats.Survival        += Statics.Specs.Trackings.Survival;
            character.Stats.Tactics         += Statics.Specs.Trackings.Tactics;
            character.Stats.Aid             += Statics.Specs.Trackings.Aid;
            character.Stats.Crafting        += Statics.Specs.Trackings.Crafting;
            character.Stats.Spot      += Statics.Specs.Trackings.Perception;
            // assets                                  Specs.Trackings
            character.Stats.Defense         += Statics.Specs.Trackings.Defense;
            character.Stats.Resist          += Statics.Specs.Trackings.Resist;
            character.Stats.Actions         += Statics.Specs.Trackings.Actions;
            character.Stats.Endurance       += Statics.Specs.Trackings.Endurance;
            character.Stats.Accretion       += Statics.Specs.Trackings.Accretion;
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

    private void SetWorth(Character character)
    {
        character.Details.Worth = _dice.Roll1dN(5) +
            (character.Stats.Strength +
            character.Stats.Constitution +
            character.Stats.Agility +
            character.Stats.Willpower +
            character.Stats.Abstract +
            character.Stats.Melee +
            character.Stats.Arcane +
            character.Stats.Psionics +
            character.Stats.Social +
            character.Stats.Hide +
            character.Stats.Survival +
            character.Stats.Tactics +
            character.Stats.Aid +
            character.Stats.Crafting +
            character.Stats.Spot +
            character.Stats.Defense +
            character.Stats.Resist +
            character.Stats.Actions +
            character.Stats.Endurance +
            character.Stats.Accretion)/10 +
            (int)(character.Details.Wealth * 0.1);
    }

    #endregion
}
