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
    (bool, int) RollVsEffort(Character character, string skill, int effortLevel, bool canLevelup, bool isFight);

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

    public (bool, int) RollVsEffort(Character character, string skill, int effortLevel, bool canLevelup, bool isFight)
    {
        if (!Statics.Stats.All.Contains(skill))
            throw new Exception("No such skill found to roll.");

        _validator.ValidateAgainstNull(character, "Character cannot be null.");
        _validator.ValidatePostiveNumber(effortLevel, "Effort level cannot be 0 or smaller.");
        _validator.ValidateString(skill, "Skill string is missing or invalid.");

        var charRoll = RollStat(character, skill, canLevelup, isFight);

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
    private int RollStat(Character character, string stat, bool canLevelup, bool isFight)
    {
        var roll = Rolld20();

        if (canLevelup && roll >= 20)
        {
            UpgradeEntityLevel(roll, character);
            LevelUp(character);
        }

        return stat switch
        {
            Statics.Stats.Strength      => isFight ? roll + character.Fights.Strength       : roll + character.Actuals.Strength,
            Statics.Stats.Constitution  => isFight ? roll + character.Fights.Constitution   : roll + character.Actuals.Constitution,
            Statics.Stats.Agility       => isFight ? roll + character.Fights.Agility        : roll + character.Actuals.Agility,
            Statics.Stats.Willpower     => isFight ? roll + character.Fights.Willpower      : roll + character.Actuals.Willpower,
            Statics.Stats.Abstract      => isFight ? roll + character.Fights.Abstract       : roll + character.Actuals.Abstract,
            Statics.Stats.Melee         => isFight ? roll + character.Fights.Melee          : roll + character.Actuals.Melee,
            Statics.Stats.Arcane        => isFight ? roll + character.Fights.Arcane         : roll + character.Actuals.Arcane,
            Statics.Stats.Psionics      => isFight ? roll + character.Fights.Psionics       : roll + character.Actuals.Psionics,
            Statics.Stats.Social        => isFight ? roll + character.Fights.Social         : roll + character.Actuals.Social,
            Statics.Stats.Hide          => isFight ? roll + character.Fights.Hide           : roll + character.Actuals.Hide,
            Statics.Stats.Survival      => isFight ? roll + character.Fights.Survival       : roll + character.Actuals.Survival,
            Statics.Stats.Tactics       => isFight ? roll + character.Fights.Tactics        : roll + character.Actuals.Tactics,
            Statics.Stats.Aid           => isFight ? roll + character.Fights.Aid            : roll + character.Actuals.Aid,
            Statics.Stats.Crafting      => isFight ? roll + character.Fights.Crafting       : roll + character.Actuals.Crafting,
            Statics.Stats.Perception    => isFight ? roll + character.Fights.Spot           : roll + character.Actuals.Spot,
            Statics.Stats.Defense       => isFight ? roll + character.Fights.Defense        : roll + character.Actuals.Defense,
            Statics.Stats.Resist        => isFight ? roll + character.Fights.Resist         : roll + character.Actuals.Resist,
            Statics.Stats.Actions       => isFight ? roll + character.Fights.Actions        : roll + character.Actuals.Actions,
            Statics.Stats.Endurance     => isFight ? roll + character.Fights.Endurance      : roll + character.Actuals.Endurance,
            Statics.Stats.Accretion     => isFight ? roll + character.Fights.Accretion      : roll + character.Actuals.Accretion,
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
