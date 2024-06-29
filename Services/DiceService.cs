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

    double Roll_vs_effort(Character character, string craft, int effort);
}

public class DiceService : IDiceService
{
    static readonly Random random = new();

    /// <summary>
    /// return a percentage of the result
    /// </summary>
    /// <param name="charVm"></param>
    /// <param name="feat"></param>
    /// <param name="effort"></param>
    /// <param name="snapshot"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public double Roll_vs_effort(Character character, string feat, int effort)
    {
        if (!Statics.Stats.All.Contains(feat))
            throw new Exception("No such stat found to roll.");

        Validators.ValidateAgainstNull(character, "Character cannot be null.");

        var charRoll = RollStats(character, feat);
        var effortRoll = Roll_1dn(effort);
        var result = charRoll - effortRoll;

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
    private int RollStats(Character character, string craft)
    {
        var roll = Roll_d20();

        UpgradeEntityLevel(roll, character);
        LevelUp(roll, character);

        return craft switch
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
        character.Details.Levelup = result * 10 * character.Details.Entitylevel;
    }
    #endregion
}
