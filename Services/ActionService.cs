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
        var source = board.GetAll().Find(s => s.Id == actions.SourceId)!;
        var target = board.GetAll().Find(s => s.Id == actions.TargetId)!;

        if (target.IsHidden)
            throw new Exception("Unable to engage your target in melee, your enemy is hidden.");

        var sourceIdentity = _snapshot.GetAllCharacters().Find(s => s.Identity.Id == source.Id)!.Identity;

        var targetIdentity = (_snapshot.GetAllCharacters().Find(s => s.Identity.Id == target.Id) ?? _snapshot.Npcs.Find(s => s.Identity.Id == target.Id))!.Identity;

        var sourceRoll = _diceService.Rolld20Character(sourceIdentity, Statics.Stats.Melee, true);
        var targetRoll = _diceService.Rolld20Character(targetIdentity, Statics.Stats.Melee, true);

        if (sourceRoll > targetRoll)
        {
            if (target.Fights.Defense > 1)
            {
                target.Fights.Defense = (int)(target.Fights.Defense * 0.5);
                board.Message = $"{source.Name} scored a hit, substantially reducing {target.Name}'s Defense.";
            }
            else if (target.Fights.Defense == 1)
            {
                target.Fights.Defense = 0;
                board.Message = $"{source.Name} scored a hit, removing {target.Name}'s Defense.";
            }
            else if (target.Fights.Hitpoints > 1)
            {
                target.Fights.Hitpoints = (int)(target.Fights.Hitpoints * 0.5);
                board.Message = $"{source.Name} scored a hit, substantially reducing {target.Name}'s Endurance.";
            }
            else
            {
                target.Fights.Hitpoints = 0;
                target.IsAlive = false;
                board.Message = $"{source.Name} scored a hit, dropping {target.Name}'s to the ground.";
            }
        } 
        else
        {
            board.Message = $"{source.Name} missed...";
        }

        source.Fights.Actions--;
    }

    #endregion
}
