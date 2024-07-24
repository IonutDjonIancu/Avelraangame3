using Models;

namespace Services;

public interface IActionService
{
    void RunCharacterActions(CharacterActions action);
}

public class ActionService : IActionService
{
    private readonly ISnapshot _snapshot;
    private readonly IDiceService _dice;

    public ActionService(
        ISnapshot snapshot,
        IDiceService dice)
    {
        _snapshot = snapshot;
        _dice = dice;
    }

    public void RunCharacterActions(CharacterActions action)
    {
        Validators.ValidateOnActionOffense(action, _snapshot);

        var board = _snapshot.Battleboards.Find(s => s.Id == action.BoardId)!;

        switch (action.ActionType)
        {
            case Statics.Battleboards.ActionTypes.Attack:
                RunAttack(action, board);
                break;
            default:
                break;
        }





    }

    #region private methods
    private void RunAttack(CharacterActions action, Battleboard board) 
    { 
        var allGuys = board.GoodGuys.Union(board.BadGuys);

        var source = allGuys.First(s => s.Identity.Id == action.SourceId);
        var target = allGuys.First(s => s.Identity.Id == action.TargetId);

        if (target.Fights.Endurance <= 0)
        {
            board.Result = "Your target is already dead.";
            return;
        }
        
        // reduce actions for source
        source.Fights.Actions -= 1;

        var result = 0.0;

        if (target.Details.Spec == Statics.Specs.Warring // for melee classes
            || target.Details.Spec == Statics.Specs.Tracking)
        {
            result = _dice.Roll_game_dice(source, Statics.Stats.Combat, target, Statics.Stats.Combat);
        }
        else if (target.Details.Spec == Statics.Specs.Sorcery) // for spellcasting classes
        {
            if (target.Fights.Accretion <= 0)
                target.Fights.Endurance -= 2;

            result = _dice.Roll_game_dice(source, Statics.Stats.Combat, target, Statics.Stats.Abstract);
            target.Fights.Accretion -= 2;
        }
        else
        {
            throw new NotImplementedException();
        }

        if (result <= 0)
        {
            board.Result = $"{source.Details.Name} missed.";
            return;
        }

        var effect = (int)(result * source.Fights.CombatEff);
        target.Fights.Endurance -= effect;

        if (target.Fights.Endurance <= 0)
        {
            target.Details.IsAlive = false;
            board.Result = $"{target.Details.Name} is mortally wounded and falls to the ground.";
        }
        else
        {
            board.Result = $"{target.Details.Name} was hit for {effect} damage.";
        }
    }

    #endregion
}
