using Models;

namespace Services;

public interface ITownhallService
{
    Board GetBoard(CharacterIdentity characterIdentity);

    Duel GenerateDuelVsNpc(CharacterIdentity characterIdentity, string effortLevelName);
}

public class TownhallService : ITownhallService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _diceService;
    private readonly ICharacterService _characterService;
    private readonly INpcService _npcService;
    private readonly IValidatorService _validatorService;

    public TownhallService(
        ISnapshot snapshot,
        IDiceService diceService,
        ICharacterService characterService,
        INpcService npcService,
        IValidatorService validatorService)
    {
        _snapshot = snapshot;
        _diceService = diceService;
        _characterService = characterService;  
        _npcService = npcService;
        _validatorService = validatorService;
    }

    public Board GetBoard(CharacterIdentity characterIdentity)
    {
        var board = _validatorService.ValidateCharacterOnGetBoard(characterIdentity);

        return board.Type switch
        {
            Statics.Boards.Types.Duel => (Duel)board,
            _ => throw new NotImplementedException(),
        };
    }

    public Duel GenerateDuelVsNpc(CharacterIdentity characterIdentity, string effortLevelName)
    {
        var character = _validatorService.ValidateCharacterOnJoiningBoard(characterIdentity);
        _characterService.SetCharacterFights(character);

        var boardId = Guid.NewGuid();

        character.Details.IsLocked = true;
        character.Details.BoardId = boardId;
        character.Details.BoardType = Statics.Boards.Types.Duel;

        var npc = _npcService.GenerateNpc([character]);

        var duel = new Duel
        {
            Id = boardId,
            GoodGuys =
            [
                character,
            ],
            BadGuys =
            [
                npc,
            ],
            RoundNr = 1,
            Type = Statics.Boards.Types.Duel
        };

        character.Details.BoardId = boardId;
        character.Details.BoardType = Statics.Boards.Types.Duel;
        character.Details.IsHidden = false;
        character.Details.IsLocked = true;

        npc.Details.BoardId = boardId;
        npc.Details.BoardType = Statics.Boards.Types.Duel;
        npc.Details.IsHidden = false;
        npc.Details.IsLocked = true;

        PrepareForFight_nonCore(duel);
        _snapshot.Boards.Add(duel);

        return duel;
    }

    #region private methods
    private void PrepareForFight_nonCore(Board board)
    {
        throw new NotImplementedException("Needs to be redone");

        //var effortLevelTop = board.EffortLevelName == Statics.EffortLevelNames.Easy ? Statics.EffortLevels.Easy : Statics.EffortLevels.Normal;
        //var effortLevel = _diceService.Roll1dN(effortLevelTop);
        //var effort = _diceService.Roll1dN(effortLevel);

        //// gg = goodGuy
        //// bg = badGuy

        //var ggChar = board.GoodGuys.OrderBy(s => s.Stats.Actual.Tactics).First();
        //var bgChar = board.BadGuys.OrderBy(s => s.Stats.Actual.Tactics).First();

        //var (ggIsSave, ggRoll) = _diceService.RollVsEffort(ggChar, Statics.Stats.Tactics, effortLevel, true);
        //var (bgIsSave, bgRoll) = _diceService.RollVsEffort(bgChar, Statics.Stats.Tactics, effortLevel, true);

        //if (ggIsSave && !bgIsSave)
        //{
        //    board.Message = "Major tactical advantage.";

        //    bgChar.Stats.Fight.Defense   = (int)(bgChar.Actuals.Defense * 0.5);
        //    bgChar.Stats.Fight.Actions -= 1;
        //}
        //else if (ggIsSave && bgIsSave && ggRoll > bgRoll)
        //{
        //    board.Message = "Moderate tactical advantage.";

        //    bgChar.Fights.Defense   = (int)(bgChar.Actuals.Defense * 0.75);
        //}
        //else if (ggIsSave && bgIsSave && ggRoll == bgRoll)
        //{
        //    board.Message = "Tactical stalemate.";
        //}
        //else if (ggIsSave && bgIsSave && ggRoll < bgRoll)
        //{
        //    board.Message = "Moderate tactical disadvantage.";

        //    ggChar.Fights.Defense   = (int)(ggChar.Actuals.Defense * 0.75);
        //}
        //else if (!ggIsSave && bgIsSave)
        //{
        //    board.Message = "Major tactical disadvantage.";

        //    ggChar.Fights.Defense   = (int)(ggChar.Actuals.Defense * 0.5);
        //    ggChar.Fights.Actions -= 1;
        //} 
        //else
        //{
        //    board.Message = "Tactically inconclusive.";
        //}
    }
    #endregion
}
