using Models;
using Newtonsoft.Json;

namespace Services;

public interface ICharacterService
{
    Character GetCharacter(CharacterIdentity identity);
    Character GetCharacter(Guid id, Guid sessionId);
    Characters GetAllAliveCharacters();

    string CreateCharacter(CreateCharacter create);
    ImportCharacterResponse ImportCharacter(ImportCharacter import);
    string ExportCharacter(CharacterIdentity identity);

    Character EquipItem(EquipItem equipItem);
    Character UnequipItem(EquipItem equipItem);
    Character SellItem(EquipItem equipItem);
    Character Levelup(CharacterLevelup levelup);
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

    public Character Levelup(CharacterLevelup levelup)
    {
        var character = Validators.ValidateLevelupAndReturn(levelup, _snapshot);

        var stat = typeof(CharacterStats).GetProperty(levelup.Attribute)!;
        var value = (int)stat.GetValue(character.Stats)!;
        stat.SetValue(character.Stats, value + 1);

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

    public Character SellItem(EquipItem equipItem)
    {
        var (item, identity) = Validators.ValidateSellItemAndReturn(equipItem, _snapshot)!;

        if (_snapshot.ItemsSold.Count >= 1000)
            _snapshot.ItemsSold.Remove(_snapshot.ItemsSold.First());

        _snapshot.ItemsSold.Add(item.Id);

        var character = GetCharacter(identity);

        var roll = _dice.Roll_vs_effort(character, Statics.Stats.Social, Statics.EffortLevels.Easy); // TODO: remove the hardcoded difficulty

        if (item.Type == Statics.Items.Types.Trinket)
        {
            character.Supplies.Regalia.Remove(item as Trinket);
        }
        else
        {
            character.Supplies.Items.Remove(item);
        }

        character.Details.Wealth += 2 + (int)((item.Value * roll + character.Actuals.SocialEff * roll) / 2);

        return GetCharacter(character.Identity);
    }

    public Character UnequipItem(EquipItem equipItem)
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

    public Character EquipItem(EquipItem equipItem)
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
        SetInventory(character);
        SetWorth(character);

        _snapshot.Characters.Add(character);

        var characterEncr = EncryptionService.EncryptString(JsonConvert.SerializeObject(character));

        return characterEncr;
    }

    public Characters GetAllAliveCharacters()
    {
        return new Characters
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
    public Character GetCharacter(CharacterIdentity identity)
    {
        return GetCharacter(identity.Id, identity.SessionId);
    }

    /// <summary>
    /// Includes character actuals.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Character GetCharacter(Guid id, Guid sessionId)
    {
        Validators.ValidateOnGetCharacter(id, sessionId);

        var character = _snapshot.Characters.FirstOrDefault(s => s.Identity.Id == id && s.Identity.SessionId == sessionId) ?? throw new Exception("No character found. Go to import first.");

        CalculateActuals(character);

        if (character.Actuals.Defense > 90)
        {
            character.Actuals.Defense = 90; // capped at 90 %
        }

        if (character.Actuals.Resist > 100)
        {
            character.Actuals.Resist = 100; // capped at 100 %
        }
        
        return character;
    }

    #region private methods
    private static void CalculateActuals(Character character)
    {
        // states
        character.Actuals.Defense      += character.Stats.Defense      + character.Inventory.Select(s => s.Stats.Defense).Sum()      + character.Regalia.Select(s => s.Stats.Defense).Sum();
        character.Actuals.Resist       += character.Stats.Resist       + character.Inventory.Select(s => s.Stats.Resist).Sum()       + character.Regalia.Select(s => s.Stats.Resist).Sum();
        character.Actuals.Actions      += character.Stats.Actions      + character.Inventory.Select(s => s.Stats.Actions).Sum()      + character.Regalia.Select(s => s.Stats.Actions).Sum();
        character.Actuals.Endurance    += character.Stats.Endurance    + character.Inventory.Select(s => s.Stats.Endurance).Sum()    + character.Regalia.Select(s => s.Stats.Endurance).Sum();
        character.Actuals.Accretion    += character.Stats.Accretion    + character.Inventory.Select(s => s.Stats.Accretion).Sum()    + character.Regalia.Select(s => s.Stats.Accretion).Sum();
        // rolls
        character.Actuals.Combat       += character.Stats.Combat       + character.Inventory.Select(s => s.Stats.Combat).Sum()       + character.Regalia.Select(s => s.Stats.Combat).Sum();
        character.Actuals.Strength     += character.Stats.Strength     + character.Inventory.Select(s => s.Stats.Strength).Sum()     + character.Regalia.Select(s => s.Stats.Strength).Sum();
        character.Actuals.Tactics      += character.Stats.Tactics      + character.Inventory.Select(s => s.Stats.Tactics).Sum()      + character.Regalia.Select(s => s.Stats.Tactics).Sum();
        character.Actuals.Athletics    += character.Stats.Athletics    + character.Inventory.Select(s => s.Stats.Athletics).Sum()    + character.Regalia.Select(s => s.Stats.Athletics).Sum();
        character.Actuals.Survival     += character.Stats.Survival     + character.Inventory.Select(s => s.Stats.Survival).Sum()     + character.Regalia.Select(s => s.Stats.Survival).Sum();
        character.Actuals.Social       += character.Stats.Social       + character.Inventory.Select(s => s.Stats.Social).Sum()       + character.Regalia.Select(s => s.Stats.Social).Sum();
        character.Actuals.Abstract     += character.Stats.Abstract     + character.Inventory.Select(s => s.Stats.Abstract).Sum()     + character.Regalia.Select(s => s.Stats.Abstract).Sum();
        character.Actuals.Psionic      += character.Stats.Psionic      + character.Inventory.Select(s => s.Stats.Psionic).Sum()      + character.Regalia.Select(s => s.Stats.Psionic).Sum();
        character.Actuals.Crafting     += character.Stats.Crafting     + character.Inventory.Select(s => s.Stats.Crafting).Sum()     + character.Regalia.Select(s => s.Stats.Crafting).Sum();
        character.Actuals.Medicine     += character.Stats.Medicine     + character.Inventory.Select(s => s.Stats.Medicine).Sum()     + character.Regalia.Select(s => s.Stats.Medicine).Sum();
        // effects                                                                                                                                                
        character.Actuals.CombatEff    += character.Stats.CombatEff    + character.Inventory.Select(s => s.Stats.CombatEff).Sum()    + character.Regalia.Select(s => s.Stats.CombatEff).Sum();
        character.Actuals.StrengthEff  += character.Stats.StrengthEff  + character.Inventory.Select(s => s.Stats.StrengthEff).Sum()  + character.Regalia.Select(s => s.Stats.StrengthEff).Sum();
        character.Actuals.TacticsEff   += character.Stats.TacticsEff   + character.Inventory.Select(s => s.Stats.TacticsEff).Sum()   + character.Regalia.Select(s => s.Stats.TacticsEff).Sum();
        character.Actuals.AthleticsEff += character.Stats.AthleticsEff + character.Inventory.Select(s => s.Stats.AthleticsEff).Sum() + character.Regalia.Select(s => s.Stats.AthleticsEff).Sum();
        character.Actuals.SurvivalEff  += character.Stats.SurvivalEff  + character.Inventory.Select(s => s.Stats.SurvivalEff).Sum()  + character.Regalia.Select(s => s.Stats.SurvivalEff).Sum();
        character.Actuals.SocialEff    += character.Stats.SocialEff    + character.Inventory.Select(s => s.Stats.SocialEff).Sum()    + character.Regalia.Select(s => s.Stats.SocialEff).Sum();
        character.Actuals.AbstractEff  += character.Stats.AbstractEff  + character.Inventory.Select(s => s.Stats.AbstractEff).Sum()  + character.Regalia.Select(s => s.Stats.AbstractEff).Sum();
        character.Actuals.PsionicEff   += character.Stats.PsionicEff   + character.Inventory.Select(s => s.Stats.PsionicEff).Sum()   + character.Regalia.Select(s => s.Stats.PsionicEff).Sum();
        character.Actuals.CraftingEff  += character.Stats.CraftingEff  + character.Inventory.Select(s => s.Stats.CraftingEff).Sum()  + character.Regalia.Select(s => s.Stats.CraftingEff).Sum();
        character.Actuals.MedicineEff  += character.Stats.MedicineEff  + character.Inventory.Select(s => s.Stats.MedicineEff).Sum()  + character.Regalia.Select(s => s.Stats.MedicineEff).Sum();
    }

    private static void SetStats(CreateCharacter create, Character character)
    {
        // races
        if (create.Race == Statics.Races.Human)
        {
            character.Stats.Defense         += Statics.Races.Humans.Defense;
            character.Stats.Resist          += Statics.Races.Humans.Resist;
            character.Stats.Actions         += Statics.Races.Humans.Actions;
            character.Stats.Endurance       += Statics.Races.Humans.Endurance;
            character.Stats.Accretion       += Statics.Races.Humans.Accretion;
            // rolls
            character.Stats.Combat          += Statics.Races.Humans.Combat;
            character.Stats.Strength        += Statics.Races.Humans.Strength;
            character.Stats.Tactics         += Statics.Races.Humans.Tactics;
            character.Stats.Athletics       += Statics.Races.Humans.Athletics;
            character.Stats.Survival        += Statics.Races.Humans.Survival;
            character.Stats.Social          += Statics.Races.Humans.Social;
            character.Stats.Abstract        += Statics.Races.Humans.Abstract;
            character.Stats.Psionic         += Statics.Races.Humans.Psionic;
            character.Stats.Crafting        += Statics.Races.Humans.Crafting;
            character.Stats.Medicine        += Statics.Races.Humans.Medicine;
            // effects
            character.Stats.CombatEff       += Statics.Races.Humans.CombatEff;
            character.Stats.StrengthEff     += Statics.Races.Humans.StrengthEff;
            character.Stats.TacticsEff      += Statics.Races.Humans.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Races.Humans.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Races.Humans.SurvivalEff;
            character.Stats.SocialEff       += Statics.Races.Humans.SocialEff;
            character.Stats.AbstractEff     += Statics.Races.Humans.AbstractEff;
            character.Stats.PsionicEff      += Statics.Races.Humans.PsionicEff;
            character.Stats.CraftingEff     += Statics.Races.Humans.CraftingEff;
            character.Stats.MedicineEff     += Statics.Races.Humans.MedicineEff;
            
        }
        else if (create.Race == Statics.Races.Elf)
        {
            character.Stats.Defense         += Statics.Races.Elfs.Defense;
            character.Stats.Resist          += Statics.Races.Elfs.Resist;
            character.Stats.Actions         += Statics.Races.Elfs.Actions;
            character.Stats.Endurance       += Statics.Races.Elfs.Endurance;
            character.Stats.Accretion       += Statics.Races.Elfs.Accretion;
            // rolls                                         
            character.Stats.Combat          += Statics.Races.Elfs.Combat;
            character.Stats.Strength        += Statics.Races.Elfs.Strength;
            character.Stats.Tactics         += Statics.Races.Elfs.Tactics;
            character.Stats.Athletics       += Statics.Races.Elfs.Athletics;
            character.Stats.Survival        += Statics.Races.Elfs.Survival;
            character.Stats.Social          += Statics.Races.Elfs.Social;
            character.Stats.Abstract        += Statics.Races.Elfs.Abstract;
            character.Stats.Psionic         += Statics.Races.Elfs.Psionic;
            character.Stats.Crafting        += Statics.Races.Elfs.Crafting;
            character.Stats.Medicine        += Statics.Races.Elfs.Medicine;
            // effects                                       
            character.Stats.CombatEff       += Statics.Races.Elfs.CombatEff;
            character.Stats.StrengthEff     += Statics.Races.Elfs.StrengthEff;
            character.Stats.TacticsEff      += Statics.Races.Elfs.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Races.Elfs.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Races.Elfs.SurvivalEff;
            character.Stats.SocialEff       += Statics.Races.Elfs.SocialEff;
            character.Stats.AbstractEff     += Statics.Races.Elfs.AbstractEff;
            character.Stats.PsionicEff      += Statics.Races.Elfs.PsionicEff;
            character.Stats.CraftingEff     += Statics.Races.Elfs.CraftingEff;
            character.Stats.MedicineEff     += Statics.Races.Elfs.MedicineEff;
        }
        else if (create.Race == Statics.Races.Dwarf)
        {
            character.Stats.Defense         += Statics.Races.Dwarfs.Defense;
            character.Stats.Resist          += Statics.Races.Dwarfs.Resist;
            character.Stats.Actions         += Statics.Races.Dwarfs.Actions;
            character.Stats.Endurance       += Statics.Races.Dwarfs.Endurance;
            character.Stats.Accretion       += Statics.Races.Dwarfs.Accretion;
            // rolls                                         
            character.Stats.Combat          += Statics.Races.Dwarfs.Combat;
            character.Stats.Strength        += Statics.Races.Dwarfs.Strength;
            character.Stats.Tactics         += Statics.Races.Dwarfs.Tactics;
            character.Stats.Athletics       += Statics.Races.Dwarfs.Athletics;
            character.Stats.Survival        += Statics.Races.Dwarfs.Survival;
            character.Stats.Social          += Statics.Races.Dwarfs.Social;
            character.Stats.Abstract        += Statics.Races.Dwarfs.Abstract;
            character.Stats.Psionic         += Statics.Races.Dwarfs.Psionic;
            character.Stats.Crafting        += Statics.Races.Dwarfs.Crafting;
            character.Stats.Medicine        += Statics.Races.Dwarfs.Medicine;
            // effects                                       
            character.Stats.CombatEff       += Statics.Races.Dwarfs.CombatEff;
            character.Stats.StrengthEff     += Statics.Races.Dwarfs.StrengthEff;
            character.Stats.TacticsEff      += Statics.Races.Dwarfs.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Races.Dwarfs.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Races.Dwarfs.SurvivalEff;
            character.Stats.SocialEff       += Statics.Races.Dwarfs.SocialEff;
            character.Stats.AbstractEff     += Statics.Races.Dwarfs.AbstractEff;
            character.Stats.PsionicEff      += Statics.Races.Dwarfs.PsionicEff;
            character.Stats.CraftingEff     += Statics.Races.Dwarfs.CraftingEff;
            character.Stats.MedicineEff     += Statics.Races.Dwarfs.MedicineEff;
        }
        else
        {
            throw new NotImplementedException();
        }

        // cultures
        if (create.Culture == Statics.Cultures.Danarian)
        {
            character.Stats.Defense         += Statics.Cultures.Danarians.Defense;
            character.Stats.Resist          += Statics.Cultures.Danarians.Resist;
            character.Stats.Actions         += Statics.Cultures.Danarians.Actions;
            character.Stats.Endurance       += Statics.Cultures.Danarians.Endurance;
            character.Stats.Accretion       += Statics.Cultures.Danarians.Accretion;
            // rolls                                   
            character.Stats.Combat          += Statics.Cultures.Danarians.Combat;
            character.Stats.Strength        += Statics.Cultures.Danarians.Strength;
            character.Stats.Tactics         += Statics.Cultures.Danarians.Tactics;
            character.Stats.Athletics       += Statics.Cultures.Danarians.Athletics;
            character.Stats.Survival        += Statics.Cultures.Danarians.Survival;
            character.Stats.Social          += Statics.Cultures.Danarians.Social;
            character.Stats.Abstract        += Statics.Cultures.Danarians.Abstract;
            character.Stats.Psionic         += Statics.Cultures.Danarians.Psionic;
            character.Stats.Crafting        += Statics.Cultures.Danarians.Crafting;
            character.Stats.Medicine        += Statics.Cultures.Danarians.Medicine;
            // effects                                 
            character.Stats.CombatEff       += Statics.Cultures.Danarians.CombatEff;
            character.Stats.StrengthEff     += Statics.Cultures.Danarians.StrengthEff;
            character.Stats.TacticsEff      += Statics.Cultures.Danarians.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Cultures.Danarians.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Cultures.Danarians.SurvivalEff;
            character.Stats.SocialEff       += Statics.Cultures.Danarians.SocialEff;
            character.Stats.AbstractEff     += Statics.Cultures.Danarians.AbstractEff;
            character.Stats.PsionicEff      += Statics.Cultures.Danarians.PsionicEff;
            character.Stats.CraftingEff     += Statics.Cultures.Danarians.CraftingEff;
            character.Stats.MedicineEff     += Statics.Cultures.Danarians.MedicineEff;
        }
        else if (create.Culture == Statics.Cultures.Highborn)
        {
            character.Stats.Defense         += Statics.Cultures.Highborns.Defense;
            character.Stats.Resist          += Statics.Cultures.Highborns.Resist;
            character.Stats.Actions         += Statics.Cultures.Highborns.Actions;
            character.Stats.Endurance       += Statics.Cultures.Highborns.Endurance;
            character.Stats.Accretion       += Statics.Cultures.Highborns.Accretion;
            // rolls                                            
            character.Stats.Combat          += Statics.Cultures.Highborns.Combat;
            character.Stats.Strength        += Statics.Cultures.Highborns.Strength;
            character.Stats.Tactics         += Statics.Cultures.Highborns.Tactics;
            character.Stats.Athletics       += Statics.Cultures.Highborns.Athletics;
            character.Stats.Survival        += Statics.Cultures.Highborns.Survival;
            character.Stats.Social          += Statics.Cultures.Highborns.Social;
            character.Stats.Abstract        += Statics.Cultures.Highborns.Abstract;
            character.Stats.Psionic         += Statics.Cultures.Highborns.Psionic;
            character.Stats.Crafting        += Statics.Cultures.Highborns.Crafting;
            character.Stats.Medicine        += Statics.Cultures.Highborns.Medicine;
            // effects                                          
            character.Stats.CombatEff       += Statics.Cultures.Highborns.CombatEff;
            character.Stats.StrengthEff     += Statics.Cultures.Highborns.StrengthEff;
            character.Stats.TacticsEff      += Statics.Cultures.Highborns.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Cultures.Highborns.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Cultures.Highborns.SurvivalEff;
            character.Stats.SocialEff       += Statics.Cultures.Highborns.SocialEff;
            character.Stats.AbstractEff     += Statics.Cultures.Highborns.AbstractEff;
            character.Stats.PsionicEff      += Statics.Cultures.Highborns.PsionicEff;
            character.Stats.CraftingEff     += Statics.Cultures.Highborns.CraftingEff;
            character.Stats.MedicineEff     += Statics.Cultures.Highborns.MedicineEff;
        }
        else if (create.Culture == Statics.Cultures.Undermountain)
        {
            character.Stats.Defense         += Statics.Cultures.Undermountains.Defense;
            character.Stats.Resist          += Statics.Cultures.Undermountains.Resist;
            character.Stats.Actions         += Statics.Cultures.Undermountains.Actions;
            character.Stats.Endurance       += Statics.Cultures.Undermountains.Endurance;
            character.Stats.Accretion       += Statics.Cultures.Undermountains.Accretion;
            // rolls                                            
            character.Stats.Combat          += Statics.Cultures.Undermountains.Combat;
            character.Stats.Strength        += Statics.Cultures.Undermountains.Strength;
            character.Stats.Tactics         += Statics.Cultures.Undermountains.Tactics;
            character.Stats.Athletics       += Statics.Cultures.Undermountains.Athletics;
            character.Stats.Survival        += Statics.Cultures.Undermountains.Survival;
            character.Stats.Social          += Statics.Cultures.Undermountains.Social;
            character.Stats.Abstract        += Statics.Cultures.Undermountains.Abstract;
            character.Stats.Psionic         += Statics.Cultures.Undermountains.Psionic;
            character.Stats.Crafting        += Statics.Cultures.Undermountains.Crafting;
            character.Stats.Medicine        += Statics.Cultures.Undermountains.Medicine;
            // effects                                          
            character.Stats.CombatEff       += Statics.Cultures.Undermountains.CombatEff;
            character.Stats.StrengthEff     += Statics.Cultures.Undermountains.StrengthEff;
            character.Stats.TacticsEff      += Statics.Cultures.Undermountains.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Cultures.Undermountains.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Cultures.Undermountains.SurvivalEff;
            character.Stats.SocialEff       += Statics.Cultures.Undermountains.SocialEff;
            character.Stats.AbstractEff     += Statics.Cultures.Undermountains.AbstractEff;
            character.Stats.PsionicEff      += Statics.Cultures.Undermountains.PsionicEff;
            character.Stats.CraftingEff     += Statics.Cultures.Undermountains.CraftingEff;
            character.Stats.MedicineEff     += Statics.Cultures.Undermountains.MedicineEff;
        }
        else
        {
            throw new NotImplementedException();
        }

        // specs
        if (create.Spec == Statics.Specs.Warring)
        {
            character.Stats.Defense         += Statics.Specs.Warrings.Defense;
            character.Stats.Resist          += Statics.Specs.Warrings.Resist;
            character.Stats.Actions         += Statics.Specs.Warrings.Actions;
            character.Stats.Endurance       += Statics.Specs.Warrings.Endurance;
            character.Stats.Accretion       += Statics.Specs.Warrings.Accretion;
            // rolls                                   
            character.Stats.Combat          += Statics.Specs.Warrings.Combat;
            character.Stats.Strength        += Statics.Specs.Warrings.Strength;
            character.Stats.Tactics         += Statics.Specs.Warrings.Tactics;
            character.Stats.Athletics       += Statics.Specs.Warrings.Athletics;
            character.Stats.Survival        += Statics.Specs.Warrings.Survival;
            character.Stats.Social          += Statics.Specs.Warrings.Social;
            character.Stats.Abstract        += Statics.Specs.Warrings.Abstract;
            character.Stats.Psionic         += Statics.Specs.Warrings.Psionic;
            character.Stats.Crafting        += Statics.Specs.Warrings.Crafting;
            character.Stats.Medicine        += Statics.Specs.Warrings.Medicine;
            // effects                                 
            character.Stats.CombatEff       += Statics.Specs.Warrings.CombatEff;
            character.Stats.StrengthEff     += Statics.Specs.Warrings.StrengthEff;
            character.Stats.TacticsEff      += Statics.Specs.Warrings.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Specs.Warrings.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Specs.Warrings.SurvivalEff;
            character.Stats.SocialEff       += Statics.Specs.Warrings.SocialEff;
            character.Stats.AbstractEff     += Statics.Specs.Warrings.AbstractEff;
            character.Stats.PsionicEff      += Statics.Specs.Warrings.PsionicEff;
            character.Stats.CraftingEff     += Statics.Specs.Warrings.CraftingEff;
            character.Stats.MedicineEff     += Statics.Specs.Warrings.MedicineEff;
        }
        else if (create.Spec == Statics.Specs.Sorcery)
        {
            character.Stats.Defense         += Statics.Specs.Sorcerys.Defense;
            character.Stats.Resist          += Statics.Specs.Sorcerys.Resist;
            character.Stats.Actions         += Statics.Specs.Sorcerys.Actions;
            character.Stats.Endurance       += Statics.Specs.Sorcerys.Endurance;
            character.Stats.Accretion       += Statics.Specs.Sorcerys.Accretion;
            // rolls                                         
            character.Stats.Combat          += Statics.Specs.Sorcerys.Combat;
            character.Stats.Strength        += Statics.Specs.Sorcerys.Strength;
            character.Stats.Tactics         += Statics.Specs.Sorcerys.Tactics;
            character.Stats.Athletics       += Statics.Specs.Sorcerys.Athletics;
            character.Stats.Survival        += Statics.Specs.Sorcerys.Survival;
            character.Stats.Social          += Statics.Specs.Sorcerys.Social;
            character.Stats.Abstract        += Statics.Specs.Sorcerys.Abstract;
            character.Stats.Psionic         += Statics.Specs.Sorcerys.Psionic;
            character.Stats.Crafting        += Statics.Specs.Sorcerys.Crafting;
            character.Stats.Medicine        += Statics.Specs.Sorcerys.Medicine;
            // effects                                       
            character.Stats.CombatEff       += Statics.Specs.Sorcerys.CombatEff;
            character.Stats.StrengthEff     += Statics.Specs.Sorcerys.StrengthEff;
            character.Stats.TacticsEff      += Statics.Specs.Sorcerys.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Specs.Sorcerys.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Specs.Sorcerys.SurvivalEff;
            character.Stats.SocialEff       += Statics.Specs.Sorcerys.SocialEff;
            character.Stats.AbstractEff     += Statics.Specs.Sorcerys.AbstractEff;
            character.Stats.PsionicEff      += Statics.Specs.Sorcerys.PsionicEff;
            character.Stats.CraftingEff     += Statics.Specs.Sorcerys.CraftingEff;
            character.Stats.MedicineEff     += Statics.Specs.Sorcerys.MedicineEff;
        }
        else if (create.Spec == Statics.Specs.Tracking)
        {
            character.Stats.Defense         += Statics.Specs.Trackings.Defense;
            character.Stats.Resist          += Statics.Specs.Trackings.Resist;
            character.Stats.Actions         += Statics.Specs.Trackings.Actions;
            character.Stats.Endurance       += Statics.Specs.Trackings.Endurance;
            character.Stats.Accretion       += Statics.Specs.Trackings.Accretion;
            // rolls                                         
            character.Stats.Combat          += Statics.Specs.Trackings.Combat;
            character.Stats.Strength        += Statics.Specs.Trackings.Strength;
            character.Stats.Tactics         += Statics.Specs.Trackings.Tactics;
            character.Stats.Athletics       += Statics.Specs.Trackings.Athletics;
            character.Stats.Survival        += Statics.Specs.Trackings.Survival;
            character.Stats.Social          += Statics.Specs.Trackings.Social;
            character.Stats.Abstract        += Statics.Specs.Trackings.Abstract;
            character.Stats.Psionic         += Statics.Specs.Trackings.Psionic;
            character.Stats.Crafting        += Statics.Specs.Trackings.Crafting;
            character.Stats.Medicine        += Statics.Specs.Trackings.Medicine;
            // effects                                       
            character.Stats.CombatEff       += Statics.Specs.Trackings.CombatEff;
            character.Stats.StrengthEff     += Statics.Specs.Trackings.StrengthEff;
            character.Stats.TacticsEff      += Statics.Specs.Trackings.TacticsEff;
            character.Stats.AthleticsEff    += Statics.Specs.Trackings.AthleticsEff;
            character.Stats.SurvivalEff     += Statics.Specs.Trackings.SurvivalEff;
            character.Stats.SocialEff       += Statics.Specs.Trackings.SocialEff;
            character.Stats.AbstractEff     += Statics.Specs.Trackings.AbstractEff;
            character.Stats.PsionicEff      += Statics.Specs.Trackings.PsionicEff;
            character.Stats.CraftingEff     += Statics.Specs.Trackings.CraftingEff;
            character.Stats.MedicineEff     += Statics.Specs.Trackings.MedicineEff;
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
            character.Stats.Combat +
            character.Stats.Strength +
            character.Stats.Tactics +
            character.Stats.Athletics +
            character.Stats.Survival +
            character.Stats.Social +
            character.Stats.Abstract +
            character.Stats.Psionic +
            character.Stats.Crafting +
            character.Stats.Medicine +
            (int)(character.Details.Wealth * 0.1);
    }
    #endregion
}

