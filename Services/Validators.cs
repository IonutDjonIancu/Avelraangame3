using Models;
using Newtonsoft.Json;

namespace Services;

internal class Validators
{
    #region general
    internal static void ValidateAgainstNull(object obj, string message)
    {
        if (obj is null)
            throw new Exception(message);
    }

    internal static void ValidateString(string str, string message)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new Exception(message);
    }
    #endregion

    #region townhall
    internal static Character ValidateCharacterOnDuel(CharacterIdentity identity, ISnapshot snapshot) 
    {
        var character = ValidateCharacterExists(identity.Id, identity.SessionId, snapshot);

        if (character.Details.BattleboardId != Guid.Empty
            && !snapshot.Duels.Exists(s => s.Id == character.Details.BattleboardId))
            throw new Exception("Unable to find battleboard duel for character.");

        character.Details.IsLocked = snapshot.Duels.Exists(s => s.Id == character.Details.BattleboardId);
        
        if (!character.Details.IsAlive)
            throw new Exception("Your character is dead.");

        return character;
    }


    #endregion

    #region characters
    internal static Character ValidateOnExportCharacter(Guid characterId, Guid sessionId, ISnapshot snapshot)
    {
        var character = ValidateCharacterExists(characterId, sessionId, snapshot);

        if (character.Details.IsLocked)
            throw new Exception("Unable to export: character is locked.");

        if (!character.Details.IsAlive)
            throw new Exception("Unable to export: character is dead.");

        return character;
    }

    internal static void ValidateOnImportCharacter(ImportCharacter import)
    {
        ValidateAgainstNull(import, "Import object is either missing or invalid.");
        ValidateString(import.CharacterString, "Import character string cannot be null.");
        var decryptString = EncryptionService.DecryptString(import.CharacterString);

        var character = JsonConvert.DeserializeObject<Character>(decryptString) ?? throw new Exception("Unable to properly deserialize character.");
        if (character.Details.IsNpc)
            throw new Exception("Cannot play an NPC character.");
    }

    internal static void ValidateOnGetCharacter(Guid id, Guid sessionId)
    {
        if (id == Guid.Empty || sessionId == Guid.Empty)
            throw new Exception("Id or SessionId are either missing or invalid.");
    }

    internal static void ValidateOnCreateCharacter(CreateCharacter character)
    {
        ValidateAgainstNull(character, "Create character is either missing or invalid.");
        ValidateString(character.Name, "Name cannot be empty.");
        ValidateString(character.Portrait, "Portrait URL cannot be empty.");
        ValidateString(character.Race, "Race cannot be empty.");
        ValidateString(character.Culture, "Culture cannot be empty.");
        ValidateString(character.Spec, "Spec cannot be empty.");
        
        if (character.Name.Length > 30) 
            throw new Exception("Name too long, 30 characters max.");
        if (!Statics.Races.All.Contains(character.Race)) 
            throw new Exception("Race not found.");
        if (!Statics.Cultures.All.Contains(character.Culture)) 
            throw new Exception("Culture not found.");
        if (!Statics.Specs.All.Contains(character.Spec)) 
            throw new Exception("Specialization not found.");
    }

    internal static Character ValidateCharacterExists(Guid characterId, Guid sessionId, ISnapshot snapshot)
    {
        var character = snapshot.Characters.FirstOrDefault(s => s.Identity.Id == characterId) ?? throw new Exception("Character not found.");

        if (character.Identity.SessionId != sessionId)
            throw new Exception("Wrong session id.");

        return character;
    }
    #endregion

    #region items
    internal static Character ValidateLevelupAndReturn(CharacterLevelup levelup, ISnapshot snapshot)
    {
        ValidateAgainstNull(levelup, "Level up cannot be null.");
        ValidateString(levelup.Attribute, "Attribute missing or invalid for levelup.");
        var character = ValidateCharacterExists(levelup.CharacterId, levelup.SessionId, snapshot);

        if (!Statics.Stats.All.Contains(levelup.Attribute))
            throw new Exception("Unable to find attribute in list of stats.");

        if (character.Details.Levelup == 0)
            throw new Exception("No lvl up points to distribute.");

        int value;

        var stat = typeof(CharacterStats).GetProperty(levelup.Attribute)!;
        value = (int)stat.GetValue(character.Stats)!;

        if (value <= 0 && character.Details.Levelup < 1)
            throw new Exception("Not enough points to distribute.");

        if (value >= 1 && character.Details.Levelup < value * 2)
            throw new Exception("Not enough points to distribute.");

        return character;
    }

    internal static (Item, CharacterIdentity) ValidateSellItemAndReturn(EquipItem equipItem, ISnapshot snapshot)
    {
        ValidateAgainstNull(equipItem, "Equip item cannot be null.");
        ValidateAgainstNull(snapshot, "Snapshot cannot be null.");
        ValidateCharacterExists(equipItem.CharacterId, equipItem.SessionId, snapshot);

        var character = snapshot.Characters.Find(s => s.Identity.Id == equipItem.CharacterId)!;

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Supplies.Items.Exists(s => s.Id == equipItem.ItemId)
            && !character.Supplies.Regalia.Exists(s => s.Id == equipItem.ItemId))
            throw new Exception("No such item found in supplies.");

        var item = character.Supplies.Items.Union(character.Supplies.Regalia).First(s => s.Id == equipItem.ItemId)!;

        if (snapshot.ItemsSold.Contains(item.Id))
            throw new Exception("This item has already been sold.");

        return (item, character.Identity);
    }

    internal static (Item, Character) ValidateUnequipItemAndReturn(EquipItem equipItem, ISnapshot snapshot)
    {
        ValidateAgainstNull(equipItem, "Equip item cannot be null.");
        ValidateAgainstNull(snapshot, "Snapshot cannot be null.");
        ValidateCharacterExists(equipItem.CharacterId, equipItem.SessionId, snapshot);

        var character = snapshot.Characters.Find(s => s.Identity.Id == equipItem.CharacterId)!;
        
        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Inventory.Exists(s => s.Id == equipItem.ItemId)
            && !character.Regalia.Exists(s => s.Id == equipItem.ItemId))
            throw new Exception("No such item found in inventory or regalia.");

        var item = character.Inventory.Union(character.Regalia).First(s => s.Id == equipItem.ItemId)!;

        return (item, character);   
    }

    internal static (Item, Character) ValidateEquipItemAndReturn(EquipItem equipItem, ISnapshot snapshot)
    {
        ValidateAgainstNull(equipItem, "Equip item cannot be null.");
        ValidateAgainstNull(snapshot, "Snapshot cannot be null.");
        ValidateCharacterExists(equipItem.CharacterId, equipItem.SessionId, snapshot);

        var character = snapshot.Characters.Find(s => s.Identity.Id == equipItem.CharacterId)!;

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Supplies.Items.Exists(s => s.Id == equipItem.ItemId)
            && !character.Supplies.Regalia.Exists(s => s.Id == equipItem.ItemId))
            throw new Exception("No such item found in supplies.");

        var item = character.Supplies.Items.Union(character.Supplies.Regalia).First(s => s.Id == equipItem.ItemId)!;

        if (item.Type == Statics.Items.Types.Trinket
            && character.Regalia.Count == 10)
        {
            throw new Exception("Regalia is full.");
        } 
        else if (item.Type != Statics.Items.Types.Trinket
            && character.Inventory.Count >= 4)
        {
            throw new Exception("Inventory is full.");
        }

        if (item.Type == Statics.Items.Types.Armour
            && character.Inventory.Count(s => s.Type == Statics.Items.Types.Armour) >= 1)
            throw new Exception("Cannot wear more than one armour.");

        if (item.Type == Statics.Items.Types.Shield
            && character.Inventory.Count(s => s.Type == Statics.Items.Types.Shield) >= 2)
            throw new Exception("Cannot use more than two shields at once.");

        if (item.Type == Statics.Items.Types.Weapon
           && character.Inventory.Count(s => s.Type == Statics.Items.Types.Weapon) >= 3)
            throw new Exception("Cannot use more than three weapons at once.");

        return (item, character);
    }

    internal static void ValidateTrinketOrItemNotNull(Item? item, Trinket? trinket)
    {
        if (item is null && trinket is null)
            throw new Exception("Both item and trinket cannot be null.");
    }
    #endregion

    #region dice
    internal static void ValidateDiceMdNRoll(int m, int n)
    {
        if (m > n)
            throw new Exception("Dice roll mdn cannot have m >= n.");
    }

    #endregion
}
