using Models;

namespace Services;

public interface IActionService
{
    void RunActionLogic(CharacterActions actions);
}

public class ActionService : IActionService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _diceService;
    private readonly IValidatorService _validatorService;

    public ActionService(
        ISnapshot snapshot,
        IDiceService dice,
        IValidatorService validatorService)
    {
        _snapshot = snapshot;
        _diceService = dice;
        _validatorService = validatorService;
    }

    public void RunActionLogic(CharacterActions actions)
    {
        _validatorService.ValidateOnRunActionLogic(actions);

        switch (actions.ActionType)
        {
            case Statics.Boards.ActionTypes.Melee:
                RunMeleeLogic(actions);
                break;
            case Statics.Boards.ActionTypes.Arcane:
                // todo
                break;
            case Statics.Boards.ActionTypes.Psionics:
                // todo
                break;
            case Statics.Boards.ActionTypes.Aid:
                // todo
                break;
            case Statics.Boards.ActionTypes.Hide:
                // todo
                break;
            case Statics.Boards.ActionTypes.Traps:
                // todo
                break;
            default:
                break;
        }
    }

    #region private methods 
    private void RunMeleeLogic(CharacterActions actions)
    {
        var board = _snapshot.Boards.Find(s => s.Id == actions.BoardId)!;
        var source = board.GetAllBoardCharacters().Find(s => s.CharacterId == actions.SourceId)!;
        var target = board.GetAllBoardCharacters().Find(s => s.CharacterId == actions.TargetId)!;

        if (target.Details.IsHidden)
            throw new Exception("Unable to engage your target in melee, your enemy is hidden.");

        var sourceRoll = _diceService.RollForCharacterVm(source, Statics.Stats.Melee, true);
        var targetRoll = _diceService.RollForCharacterVm(target, Statics.Stats.Melee, true);

        if (sourceRoll > targetRoll)
        {
            if (target.Stats.Fights.Defense > 1)
            {
                target.Stats.Fights.Defense = (int)(target.Stats.Fights.Defense * 0.5);
                board.Message = $"{source.Details.Name} scored a hit, substantially reducing {target.Details.Name}'s Defense.";
            }
            else if (target.Stats.Fights.Defense == 1)
            {
                target.Stats.Fights.Defense = 0;
                board.Message = $"{source.Details.Name} scored a hit, removing {target.Details.Name}'s Defense.";
            }
            else if (target.Stats.Fights.Endurance > 1)
            {
                target.Stats.Fights.Endurance = (int)(target.Stats.Fights.Endurance * 0.5);
                board.Message = $"{source.Details.Name} scored a hit, substantially reducing {target.Details.Name}'s Endurance.";
            }
            else
            {
                target.Stats.Fights.Endurance = 0;
                target.Details.IsAlive = false;
                board.Message = $"{source.Details.Name} scored a hit, dropping {target.Details.Name}'s to the ground.";
            }
        } 
        else
        {
            board.Message = $"{source.Details.Name} missed...";
        }

        source.Stats.Fights.Actions--;
    }

    #endregion
}
