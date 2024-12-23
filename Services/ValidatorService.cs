using Models;
using Newtonsoft.Json;
using static Models.Statics;

namespace Services;

public interface IValidatorService
{
    #region general
    void ValidateAgainstNull(object obj, string message);
    void ValidateString(string str, string message);
    void ValidateGuid(Guid guid, string message);
    void ValidatePostiveNumber(int number, string message);
    #endregion

    #region character
    void ValidateOnCreateCharacter(CreateCharacter character);
    Character ValidateCharacterExists(CharacterIdentity identity);
    Character ValidateOnExportCharacter(CharacterIdentity identity);
    Character ValidateOnGetCharacter(CharacterIdentity identity);
    void ValidateOnImportCharacter(ImportCharacter import);
    (Item, Character) ValidateEquipItem(EquipItem equipItem);
    (Item, Character) ValidateUnequipItem(EquipItem equipItem);
    (Item, Character) ValidateSellItem(EquipItem equipItem);
    (Item, Character) ValidateBuyItem(EquipItem equipItem);
    Character ValidateLevelupAndReturn(CharacterLevelup levelup);
    #endregion

    #region board
    Board ValidateCharacterOnGetBoard(CharacterIdentity identity);
    Character ValidateCharacterOnJoiningBoard(CharacterIdentity identity);
    #endregion
}

public class ValidatorService : IValidatorService
{
    private readonly ISnapshot _snapshot;

    public ValidatorService(ISnapshot snapshot)
    {
        _snapshot = snapshot;
    }

    #region general
    public void ValidateAgainstNull(object obj, string message)
    {
        if (obj is null)
            throw new Exception(message);
    }

    public void ValidateString(string str, string message)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new Exception(message);
    }

    public void ValidateGuid(Guid guid, string message)
    {
        if (guid == Guid.Empty)
            throw new Exception(message);
    }

    public void ValidatePostiveNumber(int number, string message)
    {
        if (number <= 0)
            throw new Exception(message);
    }
    #endregion

    #region character
    public Character ValidateOnGetCharacter(CharacterIdentity identity)
    {
        ValidateAgainstNull(identity, "Identity object cannot be null.");
        ValidateGuid(identity.Id, "Id is either missing or invalid.");
        ValidateGuid(identity.SessionId, "Session id is either missing or invalid.");

        return ValidateCharacterExists(identity);
    }

    public Character ValidateOnExportCharacter(CharacterIdentity identity)
    {
        var character = ValidateCharacterExists(identity);

        if (character.Details.IsLocked)
            throw new Exception("Unable to export: character is locked.");

        if (!character.Details.IsAlive)
            throw new Exception("Unable to export: character is dead.");

        return character;
    }

    public void ValidateOnImportCharacter(ImportCharacter import)
    {
        ValidateAgainstNull(import, "Import object is either missing or invalid.");
        ValidateString(import.CharacterString, "Import character string cannot be null or empty.");

        var decryptString = EncryptionService.DecryptString(import.CharacterString);

        var character = JsonConvert.DeserializeObject<Character>(decryptString) ?? throw new Exception("Unable to properly deserialize character.");

        ValidateAgainstNull(character, "Character is null after string decryption.");

        if (character.Details.IsNpc)
            throw new Exception("Cannot play an NPC character.");

        Character existingCharacter = null;

        try
        {
            existingCharacter = ValidateCharacterExists(character.Identity);
        }
        catch (Exception)
        {
            // do nothing in case character does not exist
        }

        if (existingCharacter is not null)
        {
            if (existingCharacter.Details.IsLocked)
                throw new Exception("Unable to import character, current character is locked.");
        }
    }

    public (Item, Character) ValidateEquipItem(EquipItem equipItem)
    {
        ValidateAgainstNull(equipItem, "Equip item cannot be null.");
        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = equipItem.CharacterId,
            SessionId = equipItem.SessionId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Supplies.Items.Exists(s => s.Id == equipItem.ItemId)
            && !character.Supplies.Regalia.Exists(s => s.Id == equipItem.ItemId))
            throw new Exception("No such item found in supplies.");

        var item = character.Supplies.Items.Union(character.Supplies.Regalia).First(s => s.Id == equipItem.ItemId)!;

        if (item.Type == Statics.Items.Types.Trinket
            && character.Regalia.Count >= 10)
            throw new Exception("Regalia is full.");
        else if (item.Type != Statics.Items.Types.Trinket
            && character.Inventory.Count >= 4)
            throw new Exception("Inventory is full.");

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

    public (Item, Character) ValidateUnequipItem(EquipItem equipItem)
    {
        ValidateAgainstNull(equipItem, "Unequip item cannot be null.");
        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = equipItem.CharacterId,
            SessionId = equipItem.SessionId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Inventory.Exists(s => s.Id == equipItem.ItemId)
            && !character.Regalia.Exists(s => s.Id == equipItem.ItemId))
            throw new Exception("No such item found in inventory or regalia.");

        var item = character.Inventory.Union(character.Regalia).First(s => s.Id == equipItem.ItemId)!;

        return (item, character);
    }

    public (Item, Character) ValidateSellItem(EquipItem equipItem)
    {
        ValidateAgainstNull(equipItem, "Equip item cannot be null.");
        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = equipItem.CharacterId,
            SessionId = equipItem.SessionId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Supplies.Items.Exists(s => s.Id == equipItem.ItemId)
            && !character.Supplies.Regalia.Exists(s => s.Id == equipItem.ItemId))
            throw new Exception("No such item found in supplies.");

        var item = character.Supplies.Items.Union(character.Supplies.Regalia).First(s => s.Id == equipItem.ItemId)!;

        if (_snapshot.ItemsSold.Contains(item.Id))
            throw new Exception("This item has already been sold.");

        return (item, character);
    }

    public (Item, Character) ValidateBuyItem(EquipItem itemToBuy)
    {
        ValidateAgainstNull(itemToBuy, "Equip item cannot be null.");
        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = itemToBuy.CharacterId,
            SessionId = itemToBuy.SessionId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        var item = _snapshot.Market.Find(s => s.Id == itemToBuy.ItemId) ?? throw new Exception("No such item found on market.");

        if (item.Value > character.Details.Wealth)
            throw new Exception("Not enough coin to purchase this item.");

        return (item, character);
    }

    public Character ValidateLevelupAndReturn(CharacterLevelup levelup)
    {
        ValidateAgainstNull(levelup, "Level up cannot be null.");
        ValidateString(levelup.Stat, "Stat missing or invalid for levelup.");

        if (!Statics.Stats.All.Contains(levelup.Stat))
            throw new Exception("Unable to find stat in list of stats.");

        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = levelup.CharacterId,
            SessionId = levelup.SessionId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Details.IsAlive)
            throw new Exception("Unable to export: character is dead.");

        if (character.Details.Levelup == 0)
            throw new Exception("No lvl up points to distribute.");

        var stat = typeof(CharacterStats).GetProperty(levelup.Stat)!;
        var currentValue = (int)stat.GetValue(character.Stats)!;

        if (currentValue >= character.Details.Levelup)
            throw new Exception("Current value is higher than the available number of level up points.");
        
        return character;
    }

    public Character ValidateCharacterExists(CharacterIdentity identity)
    {
        ValidateAgainstNull(identity, "Identity cannot be null.");

        var character = _snapshot.Characters.FirstOrDefault(s => s.Identity.Id == identity.Id) ?? throw new Exception("Character not found.");

        if (character.Details.IsNpc)
            return character;

        if (character.Identity.SessionId != identity.SessionId)
            throw new Exception("Wrong session id.");

        return character;
    }

    public void ValidateOnCreateCharacter(CreateCharacter character)
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

    #endregion

    #region townhall
    public Board ValidateCharacterOnGetBoard(CharacterIdentity identity)
    {
        var character = ValidateCharacterExists(identity);
        var board = _snapshot.Boards.Find(s => s.Id == character.Details.BoardId);

        if (character.Details.BoardId != Guid.Empty
            || board is null)
            throw new Exception("Unable to find board for character.");

        character.Details.IsLocked = true;

        if (!character.Details.IsAlive)
            throw new Exception("Your character is dead.");

        return board;
    }

    public Character ValidateCharacterOnJoiningBoard(CharacterIdentity identity)
    {
        var character = ValidateCharacterExists(identity);

        if (character.Details.BoardId != Guid.Empty)
            throw new Exception("Character is already present on a board.");

        if (!character.Details.IsAlive)
            throw new Exception("Character is dead and cannot join a board.");

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        return character;
    }
    #endregion
}
