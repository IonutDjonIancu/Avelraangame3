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
    int Rolld20Character(CharacterIdentity identity, string stat, bool canLevelup = false);

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
                Statics.Stats.Strength      => Roll1dN(board.GoodGuys.Select(s => s.Fights.Strength    ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Constitution  => Roll1dN(board.GoodGuys.Select(s => s.Fights.Constitution).Sum() / board.GoodGuys.Count),
                Statics.Stats.Agility       => Roll1dN(board.GoodGuys.Select(s => s.Fights.Agility     ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Willpower     => Roll1dN(board.GoodGuys.Select(s => s.Fights.Willpower   ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Abstract      => Roll1dN(board.GoodGuys.Select(s => s.Fights.Abstract    ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Melee         => Roll1dN(board.GoodGuys.Select(s => s.Fights.Melee       ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Arcane        => Roll1dN(board.GoodGuys.Select(s => s.Fights.Arcane      ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Psionics      => Roll1dN(board.GoodGuys.Select(s => s.Fights.Psionics    ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Social        => Roll1dN(board.GoodGuys.Select(s => s.Fights.Social      ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Hide          => Roll1dN(board.GoodGuys.Select(s => s.Fights.Hide        ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Survival      => Roll1dN(board.GoodGuys.Select(s => s.Fights.Survival    ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Tactics       => Roll1dN(board.GoodGuys.Select(s => s.Fights.Tactics     ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Aid           => Roll1dN(board.GoodGuys.Select(s => s.Fights.Aid         ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Crafting      => Roll1dN(board.GoodGuys.Select(s => s.Fights.Crafting    ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Perception    => Roll1dN(board.GoodGuys.Select(s => s.Fights.Perception  ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Defense       => Roll1dN(board.GoodGuys.Select(s => s.Fights.Defense     ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Actions       => Roll1dN(board.GoodGuys.Select(s => s.Fights.Actions     ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Hitpoints     => Roll1dN(board.GoodGuys.Select(s => s.Fights.Hitpoints   ).Sum() / board.GoodGuys.Count),
                Statics.Stats.Mana          => Roll1dN(board.GoodGuys.Select(s => s.Fights.Mana        ).Sum() / board.GoodGuys.Count),
                _ => throw new NotImplementedException()
            };
        }
        else // for core
        {
            return stat switch
            {
                Statics.Stats.Strength      => RollMdN(board.GoodGuys.Select(s => s.Fights.Strength    ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Strength    ).Max() + board.GoodGuys.Select(s => s.Fights.Strength    ).Max() / 2),
                Statics.Stats.Constitution  => RollMdN(board.GoodGuys.Select(s => s.Fights.Constitution).Min() / 2, board.GoodGuys.Select(s => s.Fights.Constitution).Max() + board.GoodGuys.Select(s => s.Fights.Constitution).Max() / 2),
                Statics.Stats.Agility       => RollMdN(board.GoodGuys.Select(s => s.Fights.Agility     ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Agility     ).Max() + board.GoodGuys.Select(s => s.Fights.Agility     ).Max() / 2),
                Statics.Stats.Willpower     => RollMdN(board.GoodGuys.Select(s => s.Fights.Willpower   ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Willpower   ).Max() + board.GoodGuys.Select(s => s.Fights.Willpower   ).Max() / 2),
                Statics.Stats.Abstract      => RollMdN(board.GoodGuys.Select(s => s.Fights.Abstract    ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Abstract    ).Max() + board.GoodGuys.Select(s => s.Fights.Abstract    ).Max() / 2),
                Statics.Stats.Melee         => RollMdN(board.GoodGuys.Select(s => s.Fights.Melee       ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Melee       ).Max() + board.GoodGuys.Select(s => s.Fights.Melee       ).Max() / 2),
                Statics.Stats.Arcane        => RollMdN(board.GoodGuys.Select(s => s.Fights.Arcane      ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Arcane      ).Max() + board.GoodGuys.Select(s => s.Fights.Arcane      ).Max() / 2),
                Statics.Stats.Psionics      => RollMdN(board.GoodGuys.Select(s => s.Fights.Psionics    ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Psionics    ).Max() + board.GoodGuys.Select(s => s.Fights.Psionics    ).Max() / 2),
                Statics.Stats.Social        => RollMdN(board.GoodGuys.Select(s => s.Fights.Social      ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Social      ).Max() + board.GoodGuys.Select(s => s.Fights.Social      ).Max() / 2),
                Statics.Stats.Hide          => RollMdN(board.GoodGuys.Select(s => s.Fights.Hide        ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Hide        ).Max() + board.GoodGuys.Select(s => s.Fights.Hide        ).Max() / 2),
                Statics.Stats.Survival      => RollMdN(board.GoodGuys.Select(s => s.Fights.Survival    ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Survival    ).Max() + board.GoodGuys.Select(s => s.Fights.Survival    ).Max() / 2),
                Statics.Stats.Tactics       => RollMdN(board.GoodGuys.Select(s => s.Fights.Tactics     ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Tactics     ).Max() + board.GoodGuys.Select(s => s.Fights.Tactics     ).Max() / 2),
                Statics.Stats.Aid           => RollMdN(board.GoodGuys.Select(s => s.Fights.Aid         ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Aid         ).Max() + board.GoodGuys.Select(s => s.Fights.Aid         ).Max() / 2),
                Statics.Stats.Crafting      => RollMdN(board.GoodGuys.Select(s => s.Fights.Crafting    ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Crafting    ).Max() + board.GoodGuys.Select(s => s.Fights.Crafting    ).Max() / 2),
                Statics.Stats.Perception    => RollMdN(board.GoodGuys.Select(s => s.Fights.Perception  ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Perception  ).Max() + board.GoodGuys.Select(s => s.Fights.Perception  ).Max() / 2),
                Statics.Stats.Defense       => RollMdN(board.GoodGuys.Select(s => s.Fights.Defense     ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Defense     ).Max() + board.GoodGuys.Select(s => s.Fights.Defense     ).Max() / 2),
                Statics.Stats.Actions       => RollMdN(board.GoodGuys.Select(s => s.Fights.Actions     ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Actions     ).Max() + board.GoodGuys.Select(s => s.Fights.Actions     ).Max() / 2),
                Statics.Stats.Hitpoints     => RollMdN(board.GoodGuys.Select(s => s.Fights.Hitpoints   ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Hitpoints   ).Max() + board.GoodGuys.Select(s => s.Fights.Hitpoints   ).Max() / 2),
                Statics.Stats.Mana          => RollMdN(board.GoodGuys.Select(s => s.Fights.Mana        ).Min() / 2, board.GoodGuys.Select(s => s.Fights.Mana        ).Max() + board.GoodGuys.Select(s => s.Fights.Mana        ).Max() / 2),
                _ => throw new NotImplementedException()
            };
        }
    }

    public int Rolld20Character(CharacterIdentity identity, string stat, bool canLevelup = false)
    {
        var character = _validator.ValidateCharacterExists(identity);

        _validator.ValidateRollCharacter(character, stat);

        var roll = Rolld20();

        if (canLevelup && roll >= 20)
        {
            UpgradeEntityLevel(roll, character);
            LevelUp(character);
        }

        return stat switch
        {
            Statics.Stats.Strength      => character.Details.IsLocked ? roll + character.Stats.Fights.Strength       : roll + character.Stats.Actuals.Strength,
            Statics.Stats.Constitution  => character.Details.IsLocked ? roll + character.Stats.Fights.Constitution   : roll + character.Stats.Actuals.Constitution,
            Statics.Stats.Agility       => character.Details.IsLocked ? roll + character.Stats.Fights.Agility        : roll + character.Stats.Actuals.Agility,
            Statics.Stats.Willpower     => character.Details.IsLocked ? roll + character.Stats.Fights.Willpower      : roll + character.Stats.Actuals.Willpower,
            Statics.Stats.Abstract      => character.Details.IsLocked ? roll + character.Stats.Fights.Abstract       : roll + character.Stats.Actuals.Abstract,
            Statics.Stats.Melee         => character.Details.IsLocked ? roll + character.Stats.Fights.Melee          : roll + character.Stats.Actuals.Melee,
            Statics.Stats.Arcane        => character.Details.IsLocked ? roll + character.Stats.Fights.Arcane         : roll + character.Stats.Actuals.Arcane,
            Statics.Stats.Psionics      => character.Details.IsLocked ? roll + character.Stats.Fights.Psionics       : roll + character.Stats.Actuals.Psionics,
            Statics.Stats.Social        => character.Details.IsLocked ? roll + character.Stats.Fights.Social         : roll + character.Stats.Actuals.Social,
            Statics.Stats.Hide          => character.Details.IsLocked ? roll + character.Stats.Fights.Hide           : roll + character.Stats.Actuals.Hide,
            Statics.Stats.Survival      => character.Details.IsLocked ? roll + character.Stats.Fights.Survival       : roll + character.Stats.Actuals.Survival,
            Statics.Stats.Tactics       => character.Details.IsLocked ? roll + character.Stats.Fights.Tactics        : roll + character.Stats.Actuals.Tactics,
            Statics.Stats.Aid           => character.Details.IsLocked ? roll + character.Stats.Fights.Aid            : roll + character.Stats.Actuals.Aid,
            Statics.Stats.Crafting      => character.Details.IsLocked ? roll + character.Stats.Fights.Crafting       : roll + character.Stats.Actuals.Crafting,
            Statics.Stats.Perception    => character.Details.IsLocked ? roll + character.Stats.Fights.Perception     : roll + character.Stats.Actuals.Perception,
            Statics.Stats.Defense       => character.Details.IsLocked ? roll + character.Stats.Fights.Defense        : roll + character.Stats.Actuals.Defense,
            Statics.Stats.Actions       => character.Details.IsLocked ? roll + character.Stats.Fights.Actions        : roll + character.Stats.Actuals.Actions,
            Statics.Stats.Hitpoints     => character.Details.IsLocked ? roll + character.Stats.Fights.Hitpoints      : roll + character.Stats.Actuals.Hitpoints,
            Statics.Stats.Mana          => character.Details.IsLocked ? roll + character.Stats.Fights.Mana           : roll + character.Stats.Actuals.Mana,
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
