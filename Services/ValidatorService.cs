using Models;

namespace Services;

public interface IValidatorService
{
    #region general
    void ValidateAgainstNull(object obj, string message = "");
    void ValidateString(string str, string message);
    void ValidateGuid(Guid guid, string message);
    void ValidateGuid(string guid, string message);
    #endregion

    #region dice
    void ValidatePostiveNumber(int number, string message);
    void ValidateGreaterNumber(int number1, int number2, string message = "");
    void ValidateRollCharacter(Guid characterId, string stat);
    #endregion

    #region player
    void ValidateOnPlayerCreate(string playerName);
    Player ValidateOnPlayerLogin(Guid playerId);
    void ValidatePlayerExists(Guid playerId);
    void ValidateCharacterPlayerCombination(Guid characterId, Guid playerId);
    #endregion

    #region character
    void ValidateOnGetCharacters(Guid playerId);
    void ValidateOnCharacterCreate(CreateCharacter character);
    Character ValidateOnCharacterDelete(CharacterIdentity identity);
    Character ValidateCharacterExists(Guid characterId);
    (Item, Character) ValidateEquipItem(EquipItem equipItem);
    (Item, Character) ValidateUnequipItem(EquipItem equipItem);
    (Item, Character) ValidateSellItem(EquipItem equipItem);
    (Item, Character) ValidateBuyItem(EquipItem equipItem);
    Character ValidateLevelupAndReturn(CharacterLevelup levelup);
    #endregion

    #region npc
    #endregion

    #region board
    Board ValidateCharacterOnGetBoard(CharacterIdentity identity);
    Character ValidateCharacterOnJoiningBoard(CharacterIdentity identity);
    #endregion

    #region actions
    void ValidateOnRunActionLogic(CharacterActions actions);
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

    public void ValidateRollCharacter(Guid characterId, string stat)
    {
        ValidateCharacterExists(characterId);
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
            throw new Exception("Player not found.");
    }

    public void ValidateCharacterPlayerCombination(Guid characterId, Guid playerId)
    {
        ValidateCharacterExists(characterId);

        var charactersPlayerId = _snapshot.Characters.Find(s => s.Identity.CharacterId == characterId)!.Identity.PlayerId;

        if (charactersPlayerId != playerId)
            throw new Exception("Character player mistmatch.");
    }

    public Player ValidateOnPlayerLogin(Guid playerId)
    {
        ValidateGuid(playerId);

        return _snapshot.Players.Find(s => s.Id == playerId) ?? throw new Exception("Player not found.");
    }

    public void ValidateOnPlayerCreate(string playerName)
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
    public (Item, Character) ValidateEquipItem(EquipItem equipItem)
    {
        ValidateAgainstNull(equipItem, "Equip item cannot be null.");
        ValidateGuid(equipItem.PlayerId);
        ValidateGuid(equipItem.CharacterId);
        ValidateGuid(equipItem.ItemId);

        var character = ValidateCharacterExists(equipItem.CharacterId);
        ValidateCharacterPlayerCombination(equipItem.CharacterId, equipItem.PlayerId);

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

        var character = ValidateCharacterExists(equipItem.CharacterId);
        ValidateCharacterPlayerCombination(equipItem.CharacterId, equipItem.PlayerId);

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

        var character = ValidateCharacterExists(equipItem.CharacterId);
        ValidateCharacterPlayerCombination(equipItem.CharacterId, equipItem.PlayerId);

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
        ValidateGuid(itemToBuy.PlayerId);
        ValidateGuid(itemToBuy.CharacterId);
        ValidateGuid(itemToBuy.ItemId);

        var character = ValidateCharacterExists(itemToBuy.CharacterId);
        ValidateCharacterPlayerCombination(itemToBuy.CharacterId, itemToBuy.PlayerId);

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

        var character = ValidateCharacterExists(levelup.CharacterId);
        ValidateCharacterPlayerCombination(levelup.CharacterId, levelup.PlayerId);

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

    public Character ValidateCharacterExists(Guid characterId)
    {
        ValidateGuid(characterId);

        return _snapshot.Characters.Find(s => s.Identity.CharacterId == characterId) ?? throw new Exception("Character not found.");
    }

    public void ValidateOnGetCharacters(Guid playerId)
    {
        ValidateGuid(playerId);

        if (!_snapshot.Players.Any(s => s.Id == playerId))
            throw new Exception("Player not found.");
    }

    public Character ValidateOnCharacterDelete(CharacterIdentity identity)
    {
        ValidateAgainstNull(identity);
        var character = ValidateCharacterExists(identity.CharacterId);

        if (character.Details.IsNpc)
            throw new Exception("Cannot delete an NPC character.");

        if (character.Identity.PlayerId != identity.PlayerId)
            throw new Exception("Character player mismatch.");

        return character;
    }

    public void ValidateOnCharacterCreate(CreateCharacter create)
    {
        ValidateAgainstNull(create, "Create character is either missing or invalid.");
        ValidatePlayerExists(create.PlayerId);
        ValidateString(create.Name, "Name cannot be empty.");
        ValidateString(create.Portrait, "Portrait URL cannot be empty.");
        ValidateString(create.Race, "Race cannot be empty.");
        ValidateString(create.Culture, "Culture cannot be empty.");
        ValidateString(create.Spec, "Spec cannot be empty.");

        if (_snapshot.Characters.Where(s => s.Identity.PlayerId == create.PlayerId && s.Details.IsAlive).Count() >= 10)
            throw new Exception("You can only have a maximum of 10 playable characters at any time.");

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
    
    #endregion

    #region townhall
    public Board ValidateCharacterOnGetBoard(CharacterIdentity identity)
    {
        ValidateAgainstNull(identity);
        var character = ValidateCharacterExists(identity.CharacterId);
        ValidateCharacterPlayerCombination(identity.CharacterId, identity.PlayerId);

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        if (!character.Details.IsAlive)
            throw new Exception("Your character is dead.");

        if (character.Details.BoardId != Guid.Empty)
            throw new Exception("Unable to find board for character.");
        
        return _snapshot.Boards.Find(s => s.Id == character.Details.BoardId) ?? throw new Exception("Unable to find board for character.");
    }

    public Character ValidateCharacterOnJoiningBoard(CharacterIdentity identity)
    {
        ValidateAgainstNull(identity);
        var character = ValidateCharacterExists(identity.CharacterId);
        ValidateCharacterPlayerCombination(identity.CharacterId, identity.PlayerId);

        if (character.Details.BoardId != Guid.Empty)
            throw new Exception("Character is already present on a board.");

        if (!character.Details.IsAlive)
            throw new Exception("Character is dead.");

        if (character.Details.IsLocked)
            throw new Exception("Character is locked.");

        return character;
    }
    #endregion

    #region actions
    public void ValidateOnRunActionLogic(CharacterActions actions)
    {
        ValidateAgainstNull(actions);
        ValidateGuid(actions.SourceId);
        ValidateGuid(actions.PlayerId);
        ValidateGuid(actions.TargetId);
        ValidateGuid(actions.BoardId);

        ValidateString(actions.ActionType, "Wrong action type provided.");
        if (!Statics.Boards.ActionTypes.All.Contains(actions.ActionType))
            throw new Exception("Action type not found in all action types.");

        if (actions.ActionType == Statics.Boards.ActionTypes.Melee && actions.SourceId == actions.TargetId)
            throw new Exception("You cannot attack yourself.");

        var sourceCharacter = _snapshot.Characters.Find(s => s.Identity.CharacterId == actions.SourceId) ?? throw new Exception("Source character not found.");
        var targetCharacter = _snapshot.Characters.Find(s => s.Identity.CharacterId == actions.TargetId) ?? throw new Exception("Target character not found.");

        var board = _snapshot.Boards.Find(s => s.Id == actions.BoardId) ?? throw new Exception("Board not found.");

        var source = board.GetAllBoardCharacters().Find(s => s.CharacterId == actions.SourceId)! ?? throw new Exception("Source character not found on board.");
        if (!source.Details.IsAlive)
            throw new Exception("This character is critically wounded and cannot perform any actions at the time.");

        if (board.Battlequeue.First().CharacterId != source.CharacterId)
            throw new Exception("It is not your turn.");

        if (source.Stats.Fights.Actions <= 0)
            throw new Exception("No more actions to perform for this character.");

        var target = board.GetAllBoardCharacters().Find(s => s.CharacterId == actions.TargetId)! ?? throw new Exception("Target character not found on board.");
        if (!target.Details.IsAlive)
            throw new Exception("The target is critically wounded and is out combat.");
    }


    #endregion
}
