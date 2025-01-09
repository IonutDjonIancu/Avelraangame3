using Models;

namespace Services;

public interface ITownhallService
{
    Duel GetOrGenerateDuel(CharacterIdentity characterIdentity, string effortLevelName, string boardType);

    MarketForPlayer GetMarket(Guid playerId);
}

public class TownhallService : ITownhallService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _diceService;
    private readonly ICharacterService _characterService;
    private readonly INpcService _npcService;
    private readonly IValidatorService _validatorService;
    private readonly IItemService _itemService;

    public TownhallService(
        ISnapshot snapshot,
        IDiceService diceService,
        ICharacterService characterService,
        INpcService npcService,
        IValidatorService validatorService,
        IItemService itemService)
    {
        _snapshot = snapshot;
        _diceService = diceService;
        _characterService = characterService;  
        _npcService = npcService;
        _validatorService = validatorService;
        _itemService = itemService;

        GenerateItemsForMarket();
    }

    public Duel GetOrGenerateDuel(CharacterIdentity characterIdentity, string effortLevelName, string boardType)
    {
        var character = _validatorService.ValidateCharacterExists(characterIdentity);

        if (character.Details.IsLocked)
        {
            return (Duel)GetBoard(character.Details.BoardId);
        }
        else
        {
            return (Duel)GenerateBoard(characterIdentity, effortLevelName, boardType);
        }
    }

    public MarketForPlayer GetMarket(Guid playerId)
    {
        _validatorService.ValidatePlayerExists(playerId);

        var market = new MarketForPlayer();

        _snapshot.MarketItems.ForEach(s => market.Items.ItemsList.Add(s));

        _snapshot.Players.First(s => s.Id == playerId)
            .Characters.Where(s => s.Details.IsAlive && !s.Details.IsLocked).ToList()
            .ForEach(d =>
            {
                market.Characters.CharactersList.Add(new CharacterVm
                {
                    Id = d.Identity.Id,
                    Name = d.Details.Name,
                    Portrait = d.Details.Portrait,
                    Wealth = d.Details.Wealth,
                    IsLocked = d.Details.IsLocked,
                });
            });

        return market;
    }

    #region private methods
    private Board GenerateBoard(CharacterIdentity characterIdentity, string effortLevelName, string boardType)
    {
        return boardType switch
        {
            Statics.Boards.Types.Duel => GenerateDuel(characterIdentity, effortLevelName),
            Statics.Boards.Types.Quest => throw new NotImplementedException(),
            Statics.Boards.Types.Tourney => throw new NotImplementedException(),
            _ => throw new Exception($"Unrecognized board type to generate: {boardType}")
        };
    }

    private Board GetBoard(Guid boardId)
    {
        return _snapshot.Boards.Find(s => s.Id == boardId) ?? throw new Exception($"Board with id: {boardId} not found.");
    }

    private Duel GenerateDuel(CharacterIdentity identity, string effortLevelName)
    {
        _validatorService.ValidateCharacterOnJoiningBoard(identity);

        var character = _characterService.GetCharacter(identity);
        var boardId = Guid.NewGuid();
        
        var board = new Duel
        {
            Id = boardId,
            PlayerId = identity.PlayerId,
            RoundNr = 1,
            Type = Statics.Boards.Types.Duel,
            EffortLevelName = effortLevelName
        };

        character.Details.IsLocked = true;
        character.Details.IsHidden = false;
        character.Details.BoardId = boardId;
        character.Details.BoardType = Statics.Boards.Types.Duel;
        _characterService.SetCharacterFights(character);
        
        board.GoodGuys.Add(new CharacterVm
        {
            Id = character.Identity.Id,
            Name = character.Details.Name,
            Spec = character.Details.Spec,
            IsLocked = character.Details.IsLocked,
            IsAlive = character.Details.IsAlive,
            Portrait = character.Details.Portrait,
            Roll = 1,
            Wealth = character.Details.Wealth,
            Actuals = character.Stats.Actuals,
            Fights = character.Stats.Fights,
            EntityLevel = character.Details.Entitylevel,
        });

        var npc = _npcService.GenerateNpc([character], board);
        npc.Details.IsLocked = true;
        npc.Details.IsHidden = false;
        npc.Details.BoardId = boardId;
        npc.Details.BoardType = Statics.Boards.Types.Duel;
        _characterService.SetCharacterFights(npc);

        board.BadGuys.Add(new CharacterVm
        {
            Id = npc.Identity.Id,
            Name = npc.Details.Name,
            Spec = npc.Details.Spec,
            IsLocked = npc.Details.IsLocked,
            IsAlive = npc.Details.IsAlive,
            Portrait = npc.Details.Portrait,
            Roll = 1,
            Wealth = npc.Details.Wealth,
            Actuals = npc.Stats.Actuals,
            Fights = npc.Stats.Fights,
            EntityLevel = npc.Details.Entitylevel,
        });

        PrepareFight(board, identity.PlayerId);
        PrepareBattlequeue(board, identity.PlayerId);

        _snapshot.Boards.Add(board);

        return board;
    }

    private void GenerateItemsForMarket()
    {
        // TODO: this should change every day, unless the item is reserved

        if (_snapshot.MarketItems.Count > 0)
            return;

        for (int i = 0; i < 20; i++)
        {
            _snapshot.MarketItems.Add(_itemService.GenerateRandomItem());
        }
    }

    private void PrepareBattlequeue(Board board, Guid playerId)
    {
        var allParticipants = new List<CharacterVm>();

        board.GoodGuys.ForEach(s =>
        {
            allParticipants.Add(new CharacterVm
            {
                Id = s.Id,
                Name = s.Name,
                IsLocked = s.IsLocked,
                Portrait = s.Portrait,
                Roll = _diceService.Rolld20Character(new CharacterIdentity { Id = s.Id, PlayerId = playerId }, Statics.Stats.Perception, true),
                Wealth = s.Wealth,
            });
        });

        board.BadGuys.ForEach(s =>
        {
            allParticipants.Add(new CharacterVm
            {
                Id = s.Id,
                Name = s.Name,
                IsLocked = s.IsLocked,
                Portrait = s.Portrait,
                Roll = _diceService.Rolld20Character(new CharacterIdentity { Id = s.Id, PlayerId = Guid.Empty }, Statics.Stats.Perception, true),
                Wealth = s.Wealth,
            });
        });

        board.Battlequeue = [.. allParticipants.OrderByDescending(s => s.Roll)];
    }

    private void PrepareFight(Board board, Guid playerId)
    {
        var goodGuysTactician = board.GoodGuys.OrderBy(s => s.Fights.Tactics).First();
        var badGuysTactician = board.BadGuys.OrderBy(s => s.Fights.Tactics).First();

        var effortRoll = _diceService.RollEffortRoll(board, Statics.Stats.Tactics);

        var goodGuyRoll = _diceService.Rolld20Character(new CharacterIdentity { Id = goodGuysTactician.Id, PlayerId = playerId }, Statics.Stats.Tactics, true);
        var badGuyRoll = _diceService.Rolld20Character(new CharacterIdentity { Id = badGuysTactician.Id, PlayerId = Guid.Empty }, Statics.Stats.Tactics, true);

        var isGoodGuySaveVs = goodGuyRoll > effortRoll;
        var isBadGuySaveVs = badGuyRoll > effortRoll;

        if (isGoodGuySaveVs && !isBadGuySaveVs)
        {
            board.Message = "Major tactical advantage.";

            foreach (var guy in board.BadGuys)
            {
                guy.Fights.Defense = (int)(guy.Fights.Defense * 0.5);
                guy.Fights.Actions -= (int)(guy.Fights.Actions * 0.5);
            }
        }
        else if (isGoodGuySaveVs && isBadGuySaveVs && goodGuyRoll > badGuyRoll)
        {
            board.Message = "Moderate tactical advantage.";

            foreach (var guy in board.BadGuys)
            {
                guy.Fights.Defense = (int)(guy.Fights.Defense * 0.75);
            }
        }
        else if (isGoodGuySaveVs && isBadGuySaveVs && goodGuyRoll == badGuyRoll)
        {
            board.Message = "Tactical stalemate.";
        }
        else if (isGoodGuySaveVs && isBadGuySaveVs && goodGuyRoll < badGuyRoll)
        {
            board.Message = "Moderate tactical disadvantage.";

            foreach (var guy in board.GoodGuys)
            {
                guy.Fights.Defense = (int)(guy.Fights.Defense * 0.75);
            }
        }
        else if (!isGoodGuySaveVs && isBadGuySaveVs)
        {
            board.Message = "Major tactical disadvantage.";

            foreach (var guy in board.GoodGuys)
            {
                guy.Fights.Defense = (int)(guy.Fights.Defense * 0.5);
                guy.Fights.Actions -= (int)(guy.Fights.Actions * 0.5);
            }
        }
        else
        {
            board.Message = "Tactically inconclusive.";
        }
    }
    #endregion
}
