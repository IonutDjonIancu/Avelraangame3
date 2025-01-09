using Models;

namespace Services;

public interface IDiceService
{
    /// <summary>
    /// 1d20 roll with reroll.
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    int Rolld20(int total = 0);

    /// <summary>
    /// 1d20 roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int Rolld20NoReroll();

    /// <summary>
    /// 1dN roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int Roll1dN(int n);

    /// <summary>
    /// 1d4 roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int Roll1d4();

    /// <summary>
    /// MdN roll excluding a reroll.
    /// </summary>
    /// <returns></returns>
    int RollMdN(int m, int n);

    /// <summary>
    /// Rolls a d20 with a reroll for a character + the provided stat.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="stat"></param>
    /// <param name="canLevelup"></param>
    /// <returns></returns>
    int Rolld20Character(Character character, string stat, bool canLevelup = false);

    /// <summary>
    /// Returns an effort roll based on effort level name.
    /// </summary>
    /// <param name="effortLevelName"></param>
    /// <returns></returns>
    int RollEffortRoll(Board board, string stat);
}

public class DiceService : IDiceService
{
    private readonly IValidatorService _validator;

    static readonly Random random = new();

    public DiceService(IValidatorService validator)
    {
        _validator = validator; 
    }

    public int RollEffortRoll(Board board, string stat)
    {
        if (board.EffortLevelName == Statics.EffortLevelNames.Easy)
        {
            return Roll1dN(20);
        }
        else if (board.EffortLevelName == Statics.EffortLevelNames.Normal)
        {
            return stat switch
            {
                Statics.Stats.Strength      => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Strength).Sum() / board.GoodGuys.Count),
                Statics.Stats.Constitution  => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Constitution).Sum() / board.GoodGuys.Count),
                Statics.Stats.Agility       => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Agility).Sum() / board.GoodGuys.Count),
                Statics.Stats.Willpower     => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Willpower).Sum() / board.GoodGuys.Count),
                Statics.Stats.Abstract      => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Abstract).Sum() / board.GoodGuys.Count),
                Statics.Stats.Melee         => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Melee).Sum() / board.GoodGuys.Count),
                Statics.Stats.Arcane        => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Arcane).Sum() / board.GoodGuys.Count),
                Statics.Stats.Psionics      => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Psionics).Sum() / board.GoodGuys.Count),
                Statics.Stats.Social        => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Social).Sum() / board.GoodGuys.Count),
                Statics.Stats.Hide          => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Hide).Sum() / board.GoodGuys.Count),
                Statics.Stats.Survival      => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Survival).Sum() / board.GoodGuys.Count),
                Statics.Stats.Tactics       => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Tactics).Sum() / board.GoodGuys.Count),
                Statics.Stats.Aid           => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Aid).Sum() / board.GoodGuys.Count),
                Statics.Stats.Crafting      => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Crafting).Sum() / board.GoodGuys.Count),
                Statics.Stats.Perception    => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Perception).Sum() / board.GoodGuys.Count),
                Statics.Stats.Defense       => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Defense).Sum() / board.GoodGuys.Count),
                Statics.Stats.Actions       => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Actions).Sum() / board.GoodGuys.Count),
                Statics.Stats.Hitpoints     => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Hitpoints).Sum() / board.GoodGuys.Count),
                Statics.Stats.Mana          => Roll1dN(board.GoodGuys.Select(s => s.Stats.Fight.Mana).Sum() / board.GoodGuys.Count),
                _ => throw new NotImplementedException()
            };
        }
        else // for core
        {
            return stat switch
            {
                Statics.Stats.Strength      => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Strength    ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Strength    ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Strength    ).Max() / 2),
                Statics.Stats.Constitution  => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Constitution).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Constitution).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Constitution).Max() / 2),
                Statics.Stats.Agility       => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Agility     ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Agility     ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Agility     ).Max() / 2),
                Statics.Stats.Willpower     => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Willpower   ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Willpower   ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Willpower   ).Max() / 2),
                Statics.Stats.Abstract      => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Abstract    ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Abstract    ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Abstract    ).Max() / 2),
                Statics.Stats.Melee         => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Melee       ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Melee       ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Melee       ).Max() / 2),
                Statics.Stats.Arcane        => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Arcane      ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Arcane      ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Arcane      ).Max() / 2),
                Statics.Stats.Psionics      => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Psionics    ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Psionics    ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Psionics    ).Max() / 2),
                Statics.Stats.Social        => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Social      ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Social      ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Social      ).Max() / 2),
                Statics.Stats.Hide          => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Hide        ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Hide        ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Hide        ).Max() / 2),
                Statics.Stats.Survival      => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Survival    ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Survival    ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Survival    ).Max() / 2),
                Statics.Stats.Tactics       => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Tactics     ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Tactics     ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Tactics     ).Max() / 2),
                Statics.Stats.Aid           => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Aid         ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Aid         ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Aid         ).Max() / 2),
                Statics.Stats.Crafting      => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Crafting    ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Crafting    ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Crafting    ).Max() / 2),
                Statics.Stats.Perception    => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Perception  ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Perception  ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Perception  ).Max() / 2),
                Statics.Stats.Defense       => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Defense     ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Defense     ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Defense     ).Max() / 2),
                Statics.Stats.Actions       => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Actions     ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Actions     ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Actions     ).Max() / 2),
                Statics.Stats.Hitpoints     => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Hitpoints   ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Hitpoints   ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Hitpoints   ).Max() / 2),
                Statics.Stats.Mana          => RollMdN(board.GoodGuys.Select(s => s.Stats.Fight.Mana        ).Min() / 2, board.GoodGuys.Select(s => s.Stats.Fight.Mana        ).Max() + board.GoodGuys.Select(s => s.Stats.Fight.Mana        ).Max() / 2),
                _ => throw new NotImplementedException()
            };
        }
    }

    public int Rolld20Character(Character character, string stat, bool canLevelup = false)
    {
        _validator.ValidateRollCharacter(character, stat);

        var roll = Rolld20();

        if (canLevelup && roll >= 20)
        {
            UpgradeEntityLevel(roll, character);
            LevelUp(character);
        }

        return stat switch
        {
            Statics.Stats.Strength      => character.Details.IsLocked ? roll + character.Stats.Fight.Strength       : roll + character.Stats.Actual.Strength,
            Statics.Stats.Constitution  => character.Details.IsLocked ? roll + character.Stats.Fight.Constitution   : roll + character.Stats.Actual.Constitution,
            Statics.Stats.Agility       => character.Details.IsLocked ? roll + character.Stats.Fight.Agility        : roll + character.Stats.Actual.Agility,
            Statics.Stats.Willpower     => character.Details.IsLocked ? roll + character.Stats.Fight.Willpower      : roll + character.Stats.Actual.Willpower,
            Statics.Stats.Abstract      => character.Details.IsLocked ? roll + character.Stats.Fight.Abstract       : roll + character.Stats.Actual.Abstract,
            Statics.Stats.Melee         => character.Details.IsLocked ? roll + character.Stats.Fight.Melee          : roll + character.Stats.Actual.Melee,
            Statics.Stats.Arcane        => character.Details.IsLocked ? roll + character.Stats.Fight.Arcane         : roll + character.Stats.Actual.Arcane,
            Statics.Stats.Psionics      => character.Details.IsLocked ? roll + character.Stats.Fight.Psionics       : roll + character.Stats.Actual.Psionics,
            Statics.Stats.Social        => character.Details.IsLocked ? roll + character.Stats.Fight.Social         : roll + character.Stats.Actual.Social,
            Statics.Stats.Hide          => character.Details.IsLocked ? roll + character.Stats.Fight.Hide           : roll + character.Stats.Actual.Hide,
            Statics.Stats.Survival      => character.Details.IsLocked ? roll + character.Stats.Fight.Survival       : roll + character.Stats.Actual.Survival,
            Statics.Stats.Tactics       => character.Details.IsLocked ? roll + character.Stats.Fight.Tactics        : roll + character.Stats.Actual.Tactics,
            Statics.Stats.Aid           => character.Details.IsLocked ? roll + character.Stats.Fight.Aid            : roll + character.Stats.Actual.Aid,
            Statics.Stats.Crafting      => character.Details.IsLocked ? roll + character.Stats.Fight.Crafting       : roll + character.Stats.Actual.Crafting,
            Statics.Stats.Perception    => character.Details.IsLocked ? roll + character.Stats.Fight.Perception     : roll + character.Stats.Actual.Perception,
            Statics.Stats.Defense       => character.Details.IsLocked ? roll + character.Stats.Fight.Defense        : roll + character.Stats.Actual.Defense,
            Statics.Stats.Actions       => character.Details.IsLocked ? roll + character.Stats.Fight.Actions        : roll + character.Stats.Actual.Actions,
            Statics.Stats.Hitpoints     => character.Details.IsLocked ? roll + character.Stats.Fight.Hitpoints      : roll + character.Stats.Actual.Hitpoints,
            Statics.Stats.Mana          => character.Details.IsLocked ? roll + character.Stats.Fight.Mana           : roll + character.Stats.Actual.Mana,
            _ => throw new Exception("Wrong stat provided.")
        };
    }

    public int Roll1d4()
    {
        return random.Next(1, 5);
    }

    public int Roll1dN(int n)
    {
        return random.Next(1, n + 1);   
    }

    public int RollMdN(int m, int n)
    {
        _validator.ValidateGreaterNumber(m, n);

        return random.Next(m, n + 1);
    }

    public int Rolld20NoReroll()
    {
        return random.Next(1, 21);
    }

    public int Rolld20(int total = 0)
    {
        total += random.Next(1, 21);

        if (total % 20 == 0)
        {
            return Rolld20(total);
        }

        return total;
    }

    #region private methods
    private static void UpgradeEntityLevel(int roll, Character character)
    {
        if (roll >= 20 && character.Details.Entitylevel < 2)
        {
            character.Details.Entitylevel = 2;
        }
        else if (roll >= 40 && character.Details.Entitylevel < 3)
        {
            character.Details.Entitylevel = 3;
        }
        else if (roll >= 60 && character.Details.Entitylevel < 4)
        {
            character.Details.Entitylevel = 4;
        }
        else if (roll >= 80 && character.Details.Entitylevel < 5)
        {
            character.Details.Entitylevel = 5;
        }
        else if (roll >= 100 && character.Details.Entitylevel < 6)
        {
            character.Details.Entitylevel = 6;
        }
    }

    private static void LevelUp(Character character)
    {
        character.Details.Levelup += 2 * character.Details.Entitylevel;
    }
    #endregion
}
