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
    /// Character roll versus an effort value.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="stat"></param>
    /// <param name="effortLevel"></param>
    (bool, int) RollVsEffort(Character character, string skill, int effortLevel, bool canLevelup);

    // TODO: implement rollAttack, rollRest, rollCast, rollMend
}

public class DiceService : IDiceService
{
    private readonly IValidatorService _validator;

    static readonly Random random = new();

    public DiceService(IValidatorService validator)
    {
        _validator = validator; 
    }

    public (bool, int) RollVsEffort(Character character, string skill, int effortLevel, bool canLevelup)
    {
        if (!Statics.Stats.All.Contains(skill))
            throw new Exception("No such skill found to roll.");

        _validator.ValidateAgainstNull(character, "Character cannot be null.");
        _validator.ValidatePostiveNumber(effortLevel, "Effort level cannot be 0 or smaller.");
        _validator.ValidateString(skill, "Skill string is missing or invalid.");

        var charRoll = RollStat(character, skill, canLevelup);

        return (charRoll > effortLevel, charRoll);
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
    private int RollStat(Character character, string stat, bool canLevelup)
    {
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
            Statics.Stats.Perception    => character.Details.IsLocked ? roll + character.Stats.Fight.Perception           : roll + character.Stats.Actual.Perception,
            Statics.Stats.Defense       => character.Details.IsLocked ? roll + character.Stats.Fight.Defense        : roll + character.Stats.Actual.Defense,
            Statics.Stats.Actions       => character.Details.IsLocked ? roll + character.Stats.Fight.Actions        : roll + character.Stats.Actual.Actions,
            Statics.Stats.Hitpoints     => character.Details.IsLocked ? roll + character.Stats.Fight.Hitpoints      : roll + character.Stats.Actual.Hitpoints,
            Statics.Stats.Mana          => character.Details.IsLocked ? roll + character.Stats.Fight.Mana           : roll + character.Stats.Actual.Mana,
            _ => throw new Exception("Wrong stat provided.")
        };
    }

    private static void UpgradeEntityLevel(int roll, Character character)
    {
        if (roll >= 40 && character.Details.Entitylevel < 2)
        {
            character.Details.Entitylevel = 2;
        }
        else if (roll >= 60 && character.Details.Entitylevel < 3)
        {
            character.Details.Entitylevel = 3;
        }
        else if (roll >= 80 && character.Details.Entitylevel < 4)
        {
            character.Details.Entitylevel = 4;
        }
        else if (roll >= 100 && character.Details.Entitylevel < 5)
        {
            character.Details.Entitylevel = 5;
        }
        else if (roll >= 120 && character.Details.Entitylevel < 6)
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
