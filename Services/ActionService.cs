using Models;

namespace Services;

public interface IActionService
{
    void RunActionLogic(CharacterActions actions);
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

    public void RunActionLogic(CharacterActions actions)
    {
        //var (source, target, board) = Validators.ValidateOnActionLogic(actions, _snapshot);

        switch (actions.ActionType)
        {
            case Statics.Boards.ActionTypes.Attack:
                //RunAttackLogic(source, target, board);
                break;
            case Statics.Boards.ActionTypes.Cast:
                // todo
                break;
            case Statics.Boards.ActionTypes.Mend:
                // todo
                break;
            case Statics.Boards.ActionTypes.Rest:
                // todo
                break;
            default:
                break;
        }
    }

    #region private methods 
    //private void RunAttackLogic(Character source, Character target, Board board)
    //{
    //    if (source.Identity.Id == target.Identity.Id)
    //    {
    //        board.Message = "Cannot attack yourself.";
    //        return;
    //    }

    //    if (board.GoodGuys.Any(s => s.Identity.Id == source.Identity.Id) && board.GoodGuys.Any(s => s.Identity.Id == target.Identity.Id)
    //        || board.BadGuys.Any(s => s.Identity.Id == source.Identity.Id) && board.BadGuys.Any(s => s.Identity.Id == target.Identity.Id))
    //    {
    //        board.Message = "Cannot attack your own men.";
    //        return;
    //    }

    //    source.Fights.Actions -= 1;

    //    var targetStat = "";

    //    if (target.Stats.Melee > target.Stats.Abstract && target.Stats.Melee > target.Stats.Psionics)
    //    {
    //        targetStat = Statics.Stats.Melee;
    //    }
    //    else if (target.Stats.Abstract > target.Stats.Psionics)
    //    {
    //        targetStat = Statics.Stats.Abstract;
    //    }
    //    else
    //    {
    //        targetStat = Statics.Stats.Psionics;
    //    }

    //    var sourceRoll = 1; // TODO: change this to actual dice roll

    //    if (targetStat == Statics.Stats.Melee)
    //    {
    //    }
    //    else if (targetStat == Statics.Stats.Abstract)
    //    {

    //        var result = (int)(sourceRoll * source.Fights.AbstractEff);

    //    }
    //    else
    //    {
    //        var result = (int)(sourceRoll * source.Fights.PsionicEff);

    //    }
    //}

    #endregion
}
