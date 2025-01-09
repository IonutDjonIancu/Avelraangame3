using Models;
using Newtonsoft.Json;

namespace Services;

public interface IValidatorService
{
    #region general
    void ValidateAgainstNull(object obj, string message);
    void ValidateString(string str, string message);
    void ValidateGuid(Guid guid, string message);
    void ValidateGuid(string guid, string message);
    #endregion

    #region dice
    void ValidatePostiveNumber(int number, string message);
    void ValidateGreaterNumber(int number1, int number2, string message = "");
    void ValidateRollCharacter(Character character, string stat);
    #endregion

    #region player
    void ValidateOnCreatePlayer(string playerName);
    void ValidatePlayerExists(Guid playerId);
    #endregion

    #region character
    void ValidateOnGetCharacters(Guid playerId);
    void ValidateOnCreateCharacter(CreateCharacter character);
    Character ValidateCharacterExists(CharacterIdentity identity);
    Character ValidateOnGetCharacter(CharacterIdentity identity);
    (Item, Character) ValidateEquipItem(EquipItem equipItem);
    (Item, Character) ValidateUnequipItem(EquipItem equipItem);
    (Item, Character) ValidateSellItem(EquipItem equipItem);
    (Item, Character) ValidateBuyItem(EquipItem equipItem);
    Character ValidateLevelupAndReturn(CharacterLevelup levelup);
    #endregion

    #region npc
    void ValidateListOfCharacters(List<Character> characters);    
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
    public void ValidateAgainstNull(object obj, string message = "")
    {
        if (obj is null)
            throw new Exception(string.IsNullOrWhiteSpace(message) ? "Null object passed." : message);
    }

    public void ValidateString(string str, string message)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new Exception(message);
    }

    public void ValidateGuid(Guid guid, string message = "")
    {
        if (guid == Guid.Empty)
            throw new Exception(string.IsNullOrWhiteSpace(message) ? "Invalid guid." : message);
    }

    public void ValidateGuid(string guid, string message = "")
    {
        Guid.Parse(guid);
    }

    public void ValidatePostiveNumber(int number, string message)
    {
        if (number <= 0)
            throw new Exception(message);
    }

    public void ValidateGreaterNumber(int number1, int number2, string message = "")
    {
        ValidatePostiveNumber(number1, $"Input number:{number1} cannot be negative.");
        ValidatePostiveNumber(number2, $"Input number:{number2} cannot be negative.");

        if (number1 <= number2)
            throw new Exception(string.IsNullOrWhiteSpace(message) ? $"First number: {number1} cannot be equal or smaller than second number: {number2}" : message);
    }

    public void ValidateRollCharacter(Character character, string stat)
    {
        ValidateAgainstNull(character);
        ValidateString(stat, "Stat string cannot be null or empty.");

        if (!Statics.Stats.All.Contains(stat))
            throw new Exception("Stat string not found in list of stats.");
    }
    #endregion

    #region player
    public void ValidatePlayerExists(Guid playerId)
    {
        // Ai has an empty guid
        if (playerId == Guid.Empty) return;

        if (!_snapshot.Players.Any(s => s.Id == playerId))
            throw new Exception($"Invalid player id: {playerId}");
    }

    public void ValidateOnCreatePlayer(string playerName)
    {
        ValidateString(playerName, "Player name cannot be empty.");

        if (playerName.Length > 20)
        {
            throw new Exception("Player name cannot exceed 20 characters in length.");
        }

        var player = _snapshot.Players.FirstOrDefault(p => p.Name.Equals(playerName, StringComparison.InvariantCultureIgnoreCase));

        if (player is not null)
            throw new Exception("Player with that name already exists.");
    }
    #endregion

    #region character
    public Character ValidateOnGetCharacter(CharacterIdentity identity)
    {
        ValidateAgainstNull(identity, "Identity object cannot be null.");
        ValidateGuid(identity.Id, "Id is either missing or invalid.");
        ValidateGuid(identity.PlayerId, "Session id is either missing or invalid.");

        return ValidateCharacterExists(identity);
    }

    public (Item, Character) ValidateEquipItem(EquipItem equipItem)
    {
        ValidateAgainstNull(equipItem, "Equip item cannot be null.");
        ValidateGuid(equipItem.PlayerId);
        ValidateGuid(equipItem.CharacterId);
        ValidateGuid(equipItem.ItemId);

        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = equipItem.CharacterId,
            PlayerId = equipItem.PlayerId,
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
        ValidateGuid(equipItem.PlayerId);
        ValidateGuid(equipItem.CharacterId);
        ValidateGuid(equipItem.ItemId);

        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = equipItem.CharacterId,
            PlayerId = equipItem.PlayerId,
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
        ValidateAgainstNull(equipItem, "Unequip item cannot be null.");
        ValidateGuid(equipItem.PlayerId);
        ValidateGuid(equipItem.CharacterId);
        ValidateGuid(equipItem.ItemId);

        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = equipItem.CharacterId,
            PlayerId = equipItem.PlayerId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Supplies.Items.Exists(s => s.Id == equipItem.ItemId)
            && !character.Supplies.Regalia.Exists(s => s.Id == equipItem.ItemId))
            throw new Exception("No such item found in supplies.");

        var item = character.Supplies.Items.Union(character.Supplies.Regalia).First(s => s.Id == equipItem.ItemId)!;

        return (item, character);
    }

    public (Item, Character) ValidateBuyItem(EquipItem itemToBuy)
    {
        ValidateAgainstNull(itemToBuy, "Equip item cannot be null.");
        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = itemToBuy.CharacterId,
            PlayerId = itemToBuy.PlayerId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        var item = _snapshot.MarketItems.FirstOrDefault(s => s.Id == itemToBuy.ItemId) ?? throw new Exception("No such item found on market.");

        if (item.Value > character.Details.Wealth)
            throw new Exception("Not enough coin to purchase this item.");

        return (item, character);
    }

    public Character ValidateLevelupAndReturn(CharacterLevelup levelup)
    {
        ValidateAgainstNull(levelup, "Level up cannot be null.");
        ValidateString(levelup.Stat, "Stat missing or invalid for levelup.");
        ValidateGuid(levelup.CharacterId);
        ValidateGuid(levelup.PlayerId);

        if (!Statics.Stats.All.Contains(levelup.Stat))
            throw new Exception("Unable to find stat in list of stats.");

        var character = ValidateCharacterExists(new CharacterIdentity
        {
            Id = levelup.CharacterId,
            PlayerId = levelup.PlayerId,
        });

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Details.IsAlive)
            throw new Exception("Character is dead.");

        if (character.Details.Levelup == 0)
            throw new Exception("No lvl up points to distribute.");

        var stat = typeof(CharacterStats).GetProperty(levelup.Stat)!;
        var currentValue = (int)stat.GetValue(character.Stats.Base)!;

        if (currentValue >= character.Details.Levelup)
            throw new Exception("Level up points value should be higher than the selected stat.");
        
        return character;
    }

    public Character ValidateCharacterExists(CharacterIdentity identity)
    {
        ValidateAgainstNull(identity, "Identity cannot be null.");
        ValidateGuid(identity.Id);
        ValidatePlayerExists(identity.PlayerId);

        if (identity.PlayerId == Guid.Empty)
        {
            return _snapshot.Npcs.First(s => s.Identity.Id == identity.Id);
        }
        else
        {
            return _snapshot.Players.First(s => s.Id == identity.PlayerId).Characters.FirstOrDefault(s => s.Identity.Id == identity.Id) ?? throw new Exception("Character not found.");
        }
    }

    public void ValidateOnGetCharacters(Guid playerId)
    {
        ValidateGuid(playerId);

        if (!_snapshot.Players.Any(s => s.Id == playerId))
            throw new Exception("Player not found.");
    }

    public void ValidateOnCreateCharacter(CreateCharacter create)
    {
        ValidateAgainstNull(create, "Create character is either missing or invalid.");
        ValidatePlayerExists(create.PlayerId);
        ValidateString(create.Name, "Name cannot be empty.");
        ValidateString(create.Portrait, "Portrait URL cannot be empty.");
        ValidateString(create.Race, "Race cannot be empty.");
        ValidateString(create.Culture, "Culture cannot be empty.");
        ValidateString(create.Spec, "Spec cannot be empty.");

        if (_snapshot.Players.First(s => s.Id == create.PlayerId).Characters.Where(s => s.Details.IsAlive).ToList().Count >= 10)
            throw new Exception("You can only have a maximum of 10 alive characters at any time.");

        if (create.Name.Length > 30)
            throw new Exception("Name too long, 30 characters max.");
        if (!Statics.Races.All.Contains(create.Race))
            throw new Exception("Race not found.");
        if (!Statics.Cultures.All.Contains(create.Culture))
            throw new Exception("Culture not found.");
        if (!Statics.Specs.All.Contains(create.Spec))
            throw new Exception("Specialization not found.");
    }

    #endregion

    #region npc
    public void ValidateListOfCharacters(List<Character> characters)
    {
        ValidateAgainstNull(characters);

        if (characters.Count == 0)
            throw new Exception("List of characters on generate npc cannot be empty.");
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
