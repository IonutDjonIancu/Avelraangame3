using Models;
using Newtonsoft.Json;

namespace Services;

public interface ICharacterService
{
    CharacterVm GetCharacter(Guid id, Guid sessionId);
    CharactersVm GetAllAliveCharacters();

    string CreateCharacter(CreateCharacter create);
    ImportCharacterResponse ImportCharacter(ImportCharacter import);
    string ExportCharacter(CharacterIdentity identity);

    CharacterVm EquipItem(EquipItem equipItem);
    CharacterVm UnequipItem(EquipItem equipItem);
    CharacterVm SellItem(EquipItem equipItem);
    CharacterVm Levelup(CharacterLevelup levelup);
}

public class CharacterService : ICharacterService
{
    private readonly ISnapshot _snapshot;
    private readonly IItemService _items;
    private readonly IDiceService _dice;

    public CharacterService(
        ISnapshot snapshot,
        IItemService itemService,
        IDiceService dice)
    {
        _snapshot = snapshot;
        _items = itemService;
        _dice = dice;
    }

    public CharacterVm Levelup(CharacterLevelup levelup)
    {
        var character = Validators.ValidateLevelupAndReturn(levelup, _snapshot);

        int value;

        if (Statics.Stats.All.Contains(levelup.Attribute))
        {
            var stat = typeof(CharacterStats).GetProperty(levelup.Attribute)!;
            value = (int)stat.GetValue(character.Stats)!;
            stat.SetValue(character.Stats, value + 1);
        }
        else
        {
            var craft = typeof(CharacterFeats).GetProperty(levelup.Attribute)!;
            value = (int)craft.GetValue(character.Feats)!;
            craft.SetValue(character.Feats, value + 1);
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
        var (item, character) = Validators.ValidateSellItemAndReturn(equipItem, _snapshot)!;

        if (_snapshot.ItemsSold.Count >= 1000)
            _snapshot.ItemsSold.Remove(_snapshot.ItemsSold.First());

        _snapshot.ItemsSold.Add(item.Id);

        var charVm = GetCharacter(character.Identity);

        var roll = _dice.Roll_vs_effort(charVm, Statics.Feats.Social, Statics.EffortLevels.Easy, _snapshot);

        if (item.Type == Statics.Items.Types.Trinket)
        {
            character.Supplies.Regalia.Remove(item as Trinket);
        }
        else
        {
            character.Supplies.Items.Remove(item);
        }

        character.Details.Wealth += 2 + (int)((item.Value * roll + charVm.Feats.SocialEff * roll) / 2);

        return GetCharacter(character.Identity);
    }

    public CharacterVm UnequipItem(EquipItem equipItem)
    {
        var (item, character) = Validators.ValidateUnequipItemAndReturn(equipItem, _snapshot)!;

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
        var (item, character) = Validators.ValidateEquipItemAndReturn(equipItem, _snapshot)!;

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

    public string ExportCharacter(CharacterIdentity identity)
    {
        var character = Validators.ValidateOnExportCharacter(identity.Id, identity.SessionId, _snapshot);

        _snapshot.CharactersImported.Remove(character.Identity.Id);

        return EncryptionService.EncryptString(JsonConvert.SerializeObject(character));
    }

    public ImportCharacterResponse ImportCharacter(ImportCharacter import)
    {
        Validators.ValidateOnImportCharacter(import, _snapshot);
        var decryptString = EncryptionService.DecryptString(import.CharacterString);

        var character = JsonConvert.DeserializeObject<Character>(decryptString)!;

        _snapshot.CharactersImported.Add(character.Identity.Id);

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
        SetFeats(create, character);
        SetInventory(character);
        SetWorth(character);

        _snapshot.Characters.Add(character);

        var characterEncr = EncryptionService.EncryptString(JsonConvert.SerializeObject(character));

        return characterEncr;
    }

    public CharacterVm GetCharacter(CharacterIdentity identity)
    {
        return GetCharacter(identity.Id, identity.SessionId);
    }

    public CharactersVm GetAllAliveCharacters()
    {
        return new CharactersVm
        {
            CharactersPortraits = _snapshot.Characters
                .Where(s => s.Details.IsAlive && !s.Details.IsNpc)
                .Select(s => s.Details.Portrait)
                .ToList(),
        };
    }

    /// <summary>
    /// Includes character actuals.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public CharacterVm GetCharacter(Guid id, Guid sessionId)
    {
        Validators.ValidateOnGetCharacter(id, sessionId);

        var character = _snapshot.Characters.FirstOrDefault(s => s.Identity.Id == id && s.Identity.SessionId == sessionId);

        Validators.ValidateAgainstNull(character!, "No character found. Go to import first.");

        var charVm =  new CharacterVm
        {
            Identity = new CharacterIdentityVm
            {
                Id = id,
            },
            Details = character!.Details,
            Stats = character.Stats,
            Feats = character.Feats,
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
        // stats
        character.Actuals.Stats.Defense      += character.Stats.Defense      + character.Inventory.Select(s => s.Stats.Defense).Sum()      + character.Regalia.Select(s => s.Stats.Defense).Sum();
        character.Actuals.Stats.Resist       += character.Stats.Resist       + character.Inventory.Select(s => s.Stats.Resist).Sum()       + character.Regalia.Select(s => s.Stats.Resist).Sum();
        character.Actuals.Stats.Actions      += character.Stats.Actions      + character.Inventory.Select(s => s.Stats.Actions).Sum()      + character.Regalia.Select(s => s.Stats.Actions).Sum();
        character.Actuals.Stats.Endurance    += character.Stats.Endurance    + character.Inventory.Select(s => s.Stats.Endurance).Sum()    + character.Regalia.Select(s => s.Stats.Endurance).Sum();
        character.Actuals.Stats.Accretion    += character.Stats.Accretion    + character.Inventory.Select(s => s.Stats.Accretion).Sum()    + character.Regalia.Select(s => s.Stats.Accretion).Sum();
        // feats roll                                                                                                                      
        character.Actuals.Feats.Combat       += character.Feats.Combat       + character.Inventory.Select(s => s.Feats.Combat).Sum()       + character.Regalia.Select(s => s.Feats.Combat).Sum();
        character.Actuals.Feats.Strength     += character.Feats.Strength     + character.Inventory.Select(s => s.Feats.Strength).Sum()     + character.Regalia.Select(s => s.Feats.Strength).Sum();
        character.Actuals.Feats.Tactics      += character.Feats.Tactics      + character.Inventory.Select(s => s.Feats.Tactics).Sum()      + character.Regalia.Select(s => s.Feats.Tactics).Sum();
        character.Actuals.Feats.Athletics    += character.Feats.Athletics    + character.Inventory.Select(s => s.Feats.Athletics).Sum()    + character.Regalia.Select(s => s.Feats.Athletics).Sum();
        character.Actuals.Feats.Survival     += character.Feats.Survival     + character.Inventory.Select(s => s.Feats.Survival).Sum()     + character.Regalia.Select(s => s.Feats.Survival).Sum();
        character.Actuals.Feats.Social       += character.Feats.Social       + character.Inventory.Select(s => s.Feats.Social).Sum()       + character.Regalia.Select(s => s.Feats.Social).Sum();
        character.Actuals.Feats.Abstract     += character.Feats.Abstract     + character.Inventory.Select(s => s.Feats.Abstract).Sum()     + character.Regalia.Select(s => s.Feats.Abstract).Sum();
        character.Actuals.Feats.Psionic      += character.Feats.Psionic      + character.Inventory.Select(s => s.Feats.Psionic).Sum()      + character.Regalia.Select(s => s.Feats.Psionic).Sum();
        character.Actuals.Feats.Crafting     += character.Feats.Crafting     + character.Inventory.Select(s => s.Feats.Crafting).Sum()     + character.Regalia.Select(s => s.Feats.Crafting).Sum();
        character.Actuals.Feats.Medicine     += character.Feats.Medicine     + character.Inventory.Select(s => s.Feats.Medicine).Sum()     + character.Regalia.Select(s => s.Feats.Medicine).Sum();
        // feats effect
        character.Actuals.Feats.CombatEff    += character.Feats.CombatEff    + character.Inventory.Select(s => s.Feats.CombatEff).Sum()    + character.Regalia.Select(s => s.Feats.CombatEff).Sum();
        character.Actuals.Feats.StrengthEff  += character.Feats.StrengthEff  + character.Inventory.Select(s => s.Feats.StrengthEff).Sum()  + character.Regalia.Select(s => s.Feats.StrengthEff).Sum();
        character.Actuals.Feats.TacticsEff   += character.Feats.TacticsEff   + character.Inventory.Select(s => s.Feats.TacticsEff).Sum()   + character.Regalia.Select(s => s.Feats.TacticsEff).Sum();
        character.Actuals.Feats.AthleticsEff += character.Feats.AthleticsEff + character.Inventory.Select(s => s.Feats.AthleticsEff).Sum() + character.Regalia.Select(s => s.Feats.AthleticsEff).Sum();
        character.Actuals.Feats.SurvivalEff  += character.Feats.SurvivalEff  + character.Inventory.Select(s => s.Feats.SurvivalEff).Sum()  + character.Regalia.Select(s => s.Feats.SurvivalEff).Sum();
        character.Actuals.Feats.SocialEff    += character.Feats.SocialEff    + character.Inventory.Select(s => s.Feats.SocialEff).Sum()    + character.Regalia.Select(s => s.Feats.SocialEff).Sum();
        character.Actuals.Feats.AbstractEff  += character.Feats.AbstractEff  + character.Inventory.Select(s => s.Feats.AbstractEff).Sum()  + character.Regalia.Select(s => s.Feats.AbstractEff).Sum();
        character.Actuals.Feats.PsionicEff   += character.Feats.PsionicEff   + character.Inventory.Select(s => s.Feats.PsionicEff).Sum()   + character.Regalia.Select(s => s.Feats.PsionicEff).Sum();
        character.Actuals.Feats.CraftingEff  += character.Feats.CraftingEff  + character.Inventory.Select(s => s.Feats.CraftingEff).Sum()  + character.Regalia.Select(s => s.Feats.CraftingEff).Sum();
        character.Actuals.Feats.MedicineEff  += character.Feats.MedicineEff  + character.Inventory.Select(s => s.Feats.MedicineEff).Sum()  + character.Regalia.Select(s => s.Feats.MedicineEff).Sum();
    }

    private static void SetFeats(CreateCharacter create, Character character)
    {
        // races
        if (create.Race == Statics.Races.Human)
        {
            // roll
            character.Feats.Combat      += Statics.Races.Humans.Combat;
            character.Feats.Strength    += Statics.Races.Humans.Strength;
            character.Feats.Tactics     += Statics.Races.Humans.Tactics;
            character.Feats.Athletics   += Statics.Races.Humans.Athletics;
            character.Feats.Survival    += Statics.Races.Humans.Survival;
            character.Feats.Social      += Statics.Races.Humans.Social;
            character.Feats.Abstract    += Statics.Races.Humans.Abstract;
            character.Feats.Psionic     += Statics.Races.Humans.Psionic;
            character.Feats.Crafting    += Statics.Races.Humans.Crafting;
            character.Feats.Medicine    += Statics.Races.Humans.Medicine;
            // effect
            character.Feats.CombatEff       += Statics.Races.Humans.CombatEff;
            character.Feats.StrengthEff     += Statics.Races.Humans.StrengthEff;
            character.Feats.TacticsEff      += Statics.Races.Humans.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Races.Humans.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Races.Humans.SurvivalEff;
            character.Feats.SocialEff       += Statics.Races.Humans.SocialEff;
            character.Feats.AbstractEff     += Statics.Races.Humans.AbstractEff;
            character.Feats.PsionicEff      += Statics.Races.Humans.PsionicEff;
            character.Feats.CraftingEff     += Statics.Races.Humans.CraftingEff;
            character.Feats.MedicineEff     += Statics.Races.Humans.MedicineEff;
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Feats.Combat      += Statics.Races.Elfs.Combat;
            character.Feats.Strength    += Statics.Races.Elfs.Strength;
            character.Feats.Tactics     += Statics.Races.Elfs.Tactics;
            character.Feats.Athletics   += Statics.Races.Elfs.Athletics;
            character.Feats.Survival    += Statics.Races.Elfs.Survival;
            character.Feats.Social      += Statics.Races.Elfs.Social;
            character.Feats.Abstract    += Statics.Races.Elfs.Abstract;
            character.Feats.Psionic     += Statics.Races.Elfs.Psionic;
            character.Feats.Crafting    += Statics.Races.Elfs.Crafting;
            character.Feats.Medicine    += Statics.Races.Elfs.Medicine;

            character.Feats.CombatEff       += Statics.Races.Elfs.CombatEff;
            character.Feats.StrengthEff     += Statics.Races.Elfs.StrengthEff;
            character.Feats.TacticsEff      += Statics.Races.Elfs.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Races.Elfs.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Races.Elfs.SurvivalEff;
            character.Feats.SocialEff       += Statics.Races.Elfs.SocialEff;
            character.Feats.AbstractEff     += Statics.Races.Elfs.AbstractEff;
            character.Feats.PsionicEff      += Statics.Races.Elfs.PsionicEff;
            character.Feats.CraftingEff     += Statics.Races.Elfs.CraftingEff;
            character.Feats.MedicineEff     += Statics.Races.Elfs.MedicineEff;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Feats.Combat      += Statics.Races.Dwarfs.Combat;
            character.Feats.Strength    += Statics.Races.Dwarfs.Strength;
            character.Feats.Tactics     += Statics.Races.Dwarfs.Tactics;
            character.Feats.Athletics   += Statics.Races.Dwarfs.Athletics;
            character.Feats.Survival    += Statics.Races.Dwarfs.Survival;
            character.Feats.Social      += Statics.Races.Dwarfs.Social;
            character.Feats.Abstract    += Statics.Races.Dwarfs.Abstract;
            character.Feats.Psionic     += Statics.Races.Dwarfs.Psionic;
            character.Feats.Crafting    += Statics.Races.Dwarfs.Crafting;
            character.Feats.Medicine    += Statics.Races.Dwarfs.Medicine;

            character.Feats.CombatEff       += Statics.Races.Elfs.CombatEff;
            character.Feats.StrengthEff     += Statics.Races.Elfs.StrengthEff;
            character.Feats.TacticsEff      += Statics.Races.Elfs.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Races.Elfs.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Races.Elfs.SurvivalEff;
            character.Feats.SocialEff       += Statics.Races.Elfs.SocialEff;
            character.Feats.AbstractEff     += Statics.Races.Elfs.AbstractEff;
            character.Feats.PsionicEff      += Statics.Races.Elfs.PsionicEff;
            character.Feats.CraftingEff     += Statics.Races.Elfs.CraftingEff;
            character.Feats.MedicineEff     += Statics.Races.Elfs.MedicineEff;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Feats.Combat      += Statics.Cultures.Danarians.Combat;
            character.Feats.Strength    += Statics.Cultures.Danarians.Strength;
            character.Feats.Tactics     += Statics.Cultures.Danarians.Tactics;
            character.Feats.Athletics   += Statics.Cultures.Danarians.Athletics;
            character.Feats.Survival    += Statics.Cultures.Danarians.Survival;
            character.Feats.Social      += Statics.Cultures.Danarians.Social;
            character.Feats.Abstract    += Statics.Cultures.Danarians.Abstract;
            character.Feats.Psionic     += Statics.Cultures.Danarians.Psionic;
            character.Feats.Crafting    += Statics.Cultures.Danarians.Crafting;
            character.Feats.Medicine    += Statics.Cultures.Danarians.Medicine;

            character.Feats.CombatEff       += Statics.Cultures.Danarians.CombatEff;
            character.Feats.StrengthEff     += Statics.Cultures.Danarians.StrengthEff;
            character.Feats.TacticsEff      += Statics.Cultures.Danarians.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Cultures.Danarians.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Cultures.Danarians.SurvivalEff;
            character.Feats.SocialEff       += Statics.Cultures.Danarians.SocialEff;
            character.Feats.AbstractEff     += Statics.Cultures.Danarians.AbstractEff;
            character.Feats.PsionicEff      += Statics.Cultures.Danarians.PsionicEff;
            character.Feats.CraftingEff     += Statics.Cultures.Danarians.CraftingEff;
            character.Feats.MedicineEff     += Statics.Cultures.Danarians.MedicineEff;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Feats.Combat      += Statics.Cultures.Highborns.Combat;
            character.Feats.Strength    += Statics.Cultures.Highborns.Strength;
            character.Feats.Tactics     += Statics.Cultures.Highborns.Tactics;
            character.Feats.Athletics   += Statics.Cultures.Highborns.Athletics;
            character.Feats.Survival    += Statics.Cultures.Highborns.Survival;
            character.Feats.Social      += Statics.Cultures.Highborns.Social;
            character.Feats.Abstract    += Statics.Cultures.Highborns.Abstract;
            character.Feats.Psionic     += Statics.Cultures.Highborns.Psionic;
            character.Feats.Crafting    += Statics.Cultures.Highborns.Crafting;
            character.Feats.Medicine    += Statics.Cultures.Highborns.Medicine;

            character.Feats.CombatEff       += Statics.Cultures.Highborns.CombatEff;
            character.Feats.StrengthEff     += Statics.Cultures.Highborns.StrengthEff;
            character.Feats.TacticsEff      += Statics.Cultures.Highborns.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Cultures.Highborns.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Cultures.Highborns.SurvivalEff;
            character.Feats.SocialEff       += Statics.Cultures.Highborns.SocialEff;
            character.Feats.AbstractEff     += Statics.Cultures.Highborns.AbstractEff;
            character.Feats.PsionicEff      += Statics.Cultures.Highborns.PsionicEff;
            character.Feats.CraftingEff     += Statics.Cultures.Highborns.CraftingEff;
            character.Feats.MedicineEff     += Statics.Cultures.Highborns.MedicineEff;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Feats.Combat      += Statics.Cultures.Undermountains.Combat;
            character.Feats.Strength    += Statics.Cultures.Undermountains.Strength;
            character.Feats.Tactics     += Statics.Cultures.Undermountains.Tactics;
            character.Feats.Athletics   += Statics.Cultures.Undermountains.Athletics;
            character.Feats.Survival    += Statics.Cultures.Undermountains.Survival;
            character.Feats.Social      += Statics.Cultures.Undermountains.Social;
            character.Feats.Abstract    += Statics.Cultures.Undermountains.Abstract;
            character.Feats.Psionic     += Statics.Cultures.Undermountains.Psionic;
            character.Feats.Crafting    += Statics.Cultures.Undermountains.Crafting;
            character.Feats.Medicine    += Statics.Cultures.Undermountains.Medicine;

            character.Feats.CombatEff       += Statics.Cultures.Undermountains.CombatEff;
            character.Feats.StrengthEff     += Statics.Cultures.Undermountains.StrengthEff;
            character.Feats.TacticsEff      += Statics.Cultures.Undermountains.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Cultures.Undermountains.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Cultures.Undermountains.SurvivalEff;
            character.Feats.SocialEff       += Statics.Cultures.Undermountains.SocialEff;
            character.Feats.AbstractEff     += Statics.Cultures.Undermountains.AbstractEff;
            character.Feats.PsionicEff      += Statics.Cultures.Undermountains.PsionicEff;
            character.Feats.CraftingEff     += Statics.Cultures.Undermountains.CraftingEff;
            character.Feats.MedicineEff     += Statics.Cultures.Undermountains.MedicineEff;
        }
        else
        {
            throw new NotImplementedException();
        }

        // spec
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Feats.Combat      += Statics.Specs.Warrings.Combat;
            character.Feats.Strength    += Statics.Specs.Warrings.Strength;
            character.Feats.Tactics     += Statics.Specs.Warrings.Tactics;
            character.Feats.Athletics   += Statics.Specs.Warrings.Athletics;
            character.Feats.Survival    += Statics.Specs.Warrings.Survival;
            character.Feats.Social      += Statics.Specs.Warrings.Social;
            character.Feats.Abstract    += Statics.Specs.Warrings.Abstract;
            character.Feats.Psionic     += Statics.Specs.Warrings.Psionic;
            character.Feats.Crafting    += Statics.Specs.Warrings.Crafting;
            character.Feats.Medicine    += Statics.Specs.Warrings.Medicine;

            character.Feats.CombatEff       += Statics.Specs.Warrings.CombatEff;
            character.Feats.StrengthEff     += Statics.Specs.Warrings.StrengthEff;
            character.Feats.TacticsEff      += Statics.Specs.Warrings.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Specs.Warrings.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Specs.Warrings.SurvivalEff;
            character.Feats.SocialEff       += Statics.Specs.Warrings.SocialEff;
            character.Feats.AbstractEff     += Statics.Specs.Warrings.AbstractEff;
            character.Feats.PsionicEff      += Statics.Specs.Warrings.PsionicEff;
            character.Feats.CraftingEff     += Statics.Specs.Warrings.CraftingEff;
            character.Feats.MedicineEff     += Statics.Specs.Warrings.MedicineEff;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Feats.Combat      += Statics.Specs.Sorcerys.Combat;
            character.Feats.Strength    += Statics.Specs.Sorcerys.Strength;
            character.Feats.Tactics     += Statics.Specs.Sorcerys.Tactics;
            character.Feats.Athletics   += Statics.Specs.Sorcerys.Athletics;
            character.Feats.Survival    += Statics.Specs.Sorcerys.Survival;
            character.Feats.Social      += Statics.Specs.Sorcerys.Social;
            character.Feats.Abstract    += Statics.Specs.Sorcerys.Abstract;
            character.Feats.Psionic     += Statics.Specs.Sorcerys.Psionic;
            character.Feats.Crafting    += Statics.Specs.Sorcerys.Crafting;
            character.Feats.Medicine    += Statics.Specs.Sorcerys.Medicine;

            character.Feats.CombatEff       += Statics.Specs.Sorcerys.CombatEff;
            character.Feats.StrengthEff     += Statics.Specs.Sorcerys.StrengthEff;
            character.Feats.TacticsEff      += Statics.Specs.Sorcerys.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Specs.Sorcerys.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Specs.Sorcerys.SurvivalEff;
            character.Feats.SocialEff       += Statics.Specs.Sorcerys.SocialEff;
            character.Feats.AbstractEff     += Statics.Specs.Sorcerys.AbstractEff;
            character.Feats.PsionicEff      += Statics.Specs.Sorcerys.PsionicEff;
            character.Feats.CraftingEff     += Statics.Specs.Sorcerys.CraftingEff;
            character.Feats.MedicineEff     += Statics.Specs.Sorcerys.MedicineEff;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Feats.Combat      += Statics.Specs.Trackings.Combat;
            character.Feats.Strength    += Statics.Specs.Trackings.Strength;
            character.Feats.Tactics     += Statics.Specs.Trackings.Tactics;
            character.Feats.Athletics   += Statics.Specs.Trackings.Athletics;
            character.Feats.Survival    += Statics.Specs.Trackings.Survival;
            character.Feats.Social      += Statics.Specs.Trackings.Social;
            character.Feats.Abstract    += Statics.Specs.Trackings.Abstract;
            character.Feats.Psionic     += Statics.Specs.Trackings.Psionic;
            character.Feats.Crafting    += Statics.Specs.Trackings.Crafting;
            character.Feats.Medicine    += Statics.Specs.Trackings.Medicine;

            character.Feats.CombatEff       += Statics.Specs.Trackings.CombatEff;
            character.Feats.StrengthEff     += Statics.Specs.Trackings.StrengthEff;
            character.Feats.TacticsEff      += Statics.Specs.Trackings.TacticsEff;
            character.Feats.AthleticsEff    += Statics.Specs.Trackings.AthleticsEff;
            character.Feats.SurvivalEff     += Statics.Specs.Trackings.SurvivalEff;
            character.Feats.SocialEff       += Statics.Specs.Trackings.SocialEff;
            character.Feats.AbstractEff     += Statics.Specs.Trackings.AbstractEff;
            character.Feats.PsionicEff      += Statics.Specs.Trackings.PsionicEff;
            character.Feats.CraftingEff     += Statics.Specs.Trackings.CraftingEff;
            character.Feats.MedicineEff     += Statics.Specs.Trackings.MedicineEff;
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
            character.Stats.Defense     += Statics.Races.Humans.Defense;
            character.Stats.Resist      += Statics.Races.Humans.Resist;
            character.Stats.Actions     += Statics.Races.Humans.Actions;
            character.Stats.Endurance   += Statics.Races.Humans.Endurance;
            character.Stats.Accretion   += Statics.Races.Humans.Accretion;
            
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Stats.Defense     += Statics.Races.Elfs.Defense;
            character.Stats.Resist      += Statics.Races.Elfs.Resist;
            character.Stats.Actions     += Statics.Races.Elfs.Actions;
            character.Stats.Endurance   += Statics.Races.Elfs.Endurance;
            character.Stats.Accretion   += Statics.Races.Elfs.Accretion;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Stats.Defense     += Statics.Races.Dwarfs.Defense;
            character.Stats.Resist      += Statics.Races.Dwarfs.Resist;
            character.Stats.Actions     += Statics.Races.Dwarfs.Actions;
            character.Stats.Endurance   += Statics.Races.Dwarfs.Endurance;
            character.Stats.Accretion   += Statics.Races.Dwarfs.Accretion;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Stats.Defense     += Statics.Cultures.Danarians.Defense;
            character.Stats.Resist      += Statics.Cultures.Danarians.Resist;
            character.Stats.Actions     += Statics.Cultures.Danarians.Actions;
            character.Stats.Endurance   += Statics.Cultures.Danarians.Endurance;
            character.Stats.Accretion   += Statics.Cultures.Danarians.Accretion;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Stats.Defense     += Statics.Cultures.Highborns.Defense;
            character.Stats.Resist      += Statics.Cultures.Highborns.Resist;
            character.Stats.Actions     += Statics.Cultures.Highborns.Actions;
            character.Stats.Endurance   += Statics.Cultures.Highborns.Endurance;
            character.Stats.Accretion   += Statics.Cultures.Highborns.Accretion;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Stats.Defense     += Statics.Cultures.Undermountains.Defense;
            character.Stats.Resist      += Statics.Cultures.Undermountains.Resist;
            character.Stats.Actions     += Statics.Cultures.Undermountains.Actions;
            character.Stats.Endurance   += Statics.Cultures.Undermountains.Endurance;
            character.Stats.Accretion   += Statics.Cultures.Undermountains.Accretion;
        }
        else
        {
            throw new NotImplementedException();
        }

        // specs
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Stats.Defense     += Statics.Specs.Warrings.Defense;
            character.Stats.Resist      += Statics.Specs.Warrings.Resist;
            character.Stats.Actions     += Statics.Specs.Warrings.Actions;
            character.Stats.Endurance   += Statics.Specs.Warrings.Endurance;
            character.Stats.Accretion   += Statics.Specs.Warrings.Accretion;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Stats.Defense     += Statics.Specs.Sorcerys.Defense;
            character.Stats.Resist      += Statics.Specs.Sorcerys.Resist;
            character.Stats.Actions     += Statics.Specs.Sorcerys.Actions;
            character.Stats.Endurance   += Statics.Specs.Sorcerys.Endurance;
            character.Stats.Accretion   += Statics.Specs.Sorcerys.Accretion;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Stats.Defense     += Statics.Specs.Trackings.Defense;
            character.Stats.Resist      += Statics.Specs.Trackings.Resist;
            character.Stats.Actions     += Statics.Specs.Trackings.Actions;
            character.Stats.Endurance   += Statics.Specs.Trackings.Endurance;
            character.Stats.Accretion   += Statics.Specs.Trackings.Accretion;
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
        character.Details.BattleboardId = Guid.Empty;
        character.Details.BattleboardType = string.Empty;
    }

    private void SetInventory(Character character)
    {
        character.Inventory.Add(_items.GenerateSpecificItem(Statics.Items.Types.Weapon));

        var trinket = (Trinket)_items.GenerateSpecificItem(Statics.Items.Types.Trinket);
        character.Regalia.Add(trinket);
    }

    private void SetWorth(Character character)
    {
        character.Details.Worth = _dice.Roll_d20_no_rr() +
            character.Stats.Defense +
            character.Stats.Resist +
            character.Stats.Actions +
            character.Stats.Endurance +
            character.Stats.Accretion +
            character.Feats.Combat +
            character.Feats.Strength +
            character.Feats.Tactics +
            character.Feats.Athletics +
            character.Feats.Survival +
            character.Feats.Social +
            character.Feats.Abstract +
            character.Feats.Psionic +
            character.Feats.Crafting +
            character.Feats.Medicine +
            (int)(character.Details.Wealth * 0.1);
    }
    #endregion
}

