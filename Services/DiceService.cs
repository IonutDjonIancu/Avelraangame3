using Models;

namespace Services;

public interface IDiceService
{
    /// <summary>
    /// With reroll.
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    int Roll_d20(int total = 0);

    /// <summary>
    /// No reroll.
    /// </summary>
    /// <returns></returns>
    int Roll_d20_no_rr();

    /// <summary>
    /// No reroll.
    /// </summary>
    /// <returns></returns>
    int Roll_1dn(int n);

    /// <summary>
    /// No reroll.
    /// </summary>
    /// <returns></returns>
    int Roll_mdn(int m, int n);

    /// <summary>
    /// Returns a percentage of the result for rolling against effort.
    /// </summary>
    /// <param name="charVm"></param>
    /// <param name="stat"></param>
    /// <param name="effort"></param>
    /// <param name="snapshot"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    double Roll_effort_dice(Character character, string stat, int effort);

    double Roll_game_dice(Character character1, string stat1, Character character2, string stat2);
}

public class DiceService : IDiceService
{
    static readonly Random random = new();

    public double Roll_game_dice(Character character1, string stat1, Character character2, string stat2)
    {
        if (!Statics.Stats.All.Contains(stat1))
            throw new Exception($"No such stat found to roll: {stat1}");
        if (!Statics.Stats.All.Contains(stat2))
            throw new Exception($"No such stat found to roll: {stat2}");
        Validators.ValidateAgainstNull(character1, "Character cannot be null.");
        Validators.ValidateAgainstNull(character2, "Character cannot be null.");

        var char1Roll = RollStats(character1, stat1, true);
        var char2Roll = RollStats(character2, stat2, false);

        var result = char1Roll - char2Roll;

        return GetPercentageForEffect(result);
    }
    
    public double Roll_effort_dice(Character character, string stat, int effort)
    {
        if (!Statics.Stats.All.Contains(stat))
            throw new Exception("No such stat found to roll.");

        Validators.ValidateAgainstNull(character, "Character cannot be null.");

        var charRoll = RollStats(character, stat, true);
        var effortRoll = Roll_1dn(effort);
        var result = charRoll - effortRoll;

        return GetPercentageForEffect(result);
    }

    public int Roll_1dn(int n)
    {
        return random.Next(1, n + 1);   
    }

    public int Roll_mdn(int m, int n)
    {
        Validators.ValidateDiceMdNRoll(m, n);

        return random.Next(m, n + 1);
    }

    public int Roll_d20_no_rr()
    {
        return random.Next(1, 21);
    }

    public int Roll_d20(int total = 0)
    {
        total += random.Next(1, 21);

        if (total % 20 == 0)
        {
            return Roll_d20(total);
        }

        return total;
    }

    #region private methods
    private int RollStats(Character character, string stat, bool isLevelup)
    {
        var roll = Roll_d20();

        if (isLevelup)
        {
            UpgradeEntityLevel(roll, character);
            LevelUp(roll, character);
        }

        return stat switch
        {
            Statics.Stats.Combat    => Roll_d20() + character.Actuals.Combat,
            Statics.Stats.Strength  => Roll_d20() + character.Actuals.Strength,
            Statics.Stats.Tactics   => Roll_d20() + character.Actuals.Tactics,
            Statics.Stats.Athletics => Roll_d20() + character.Actuals.Athletics,
            Statics.Stats.Survival  => Roll_d20() + character.Actuals.Survival,
            Statics.Stats.Social    => Roll_d20() + character.Actuals.Social,
            Statics.Stats.Abstract  => Roll_d20() + character.Actuals.Abstract,
            Statics.Stats.Psionic   => Roll_d20() + character.Actuals.Psionic,
            Statics.Stats.Crafting  => Roll_d20() + character.Actuals.Crafting,
            Statics.Stats.Medicine  => Roll_d20() + character.Actuals.Medicine,
            _ => throw new Exception("Wrong stat provided.")
        };
    }

    private static double GetPercentageForEffect(int result)
    {
        if (result <= 0)
        {
            return 0.00;
        }
        else if (result <= 4)
        {
            return 0.1;
        }
        else if (result <= 8)
        {
            return 0.25;
        }
        else if (result <= 12)
        {
            return 0.5;
        }
        else if (result <= 16)
        {
            return 0.75;
        }
        else if (result <= 20)
        {
            return 1;
        }
        else
        {
            return result * 5 / 100; // kept like this for dice faces reminder of old rules
        }
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

    private static void LevelUp(int roll, Character character)
    {
        var result = (int)(roll / 20);
        character.Details.Levelup += result * 10 * character.Details.Entitylevel;
    }
    #endregion
}
