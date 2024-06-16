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

    double Roll_vs_effort(CharacterVm charVm, string craft, int effort);
}

public class DiceService : IDiceService
{
    static readonly Random random = new();

    public double Roll_vs_effort(CharacterVm charVm, string craft, int effort)
    {
        if (!Statics.Crafts.All.Contains(craft))
            throw new Exception("No such craft found to roll.");

        Validators.ValidateAgainstNull(charVm, "CharacterVm cannot be null.");

        var charRoll = RollCraft(charVm, craft);
        var result = effort - charRoll;

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
    private int RollCraft(CharacterVm charVm, string craft)
    {
        return craft switch
        {
            Statics.Crafts.Combat => Roll_d20() + charVm.Actuals.Crafts.Combat,
            Statics.Crafts.Arcane => Roll_d20() + charVm.Actuals.Crafts.Arcane,
            Statics.Crafts.Alchemy => Roll_d20() + charVm.Actuals.Crafts.Alchemy,
            Statics.Crafts.Psionics => Roll_d20() + charVm.Actuals.Crafts.Psionics,
            Statics.Crafts.Hunting => Roll_d20() + charVm.Actuals.Crafts.Hunting,
            Statics.Crafts.Advocacy => Roll_d20() + charVm.Actuals.Crafts.Advocacy,
            Statics.Crafts.Mercantile => Roll_d20() + charVm.Actuals.Crafts.Mercantile,
            Statics.Crafts.Tactics => Roll_d20() + charVm.Actuals.Crafts.Tactics,
            Statics.Crafts.Traveling => Roll_d20() + charVm.Actuals.Crafts.Traveling,
            Statics.Crafts.Sailing => Roll_d20() + charVm.Actuals.Crafts.Sailing,
            Statics.Crafts.Medicine => Roll_d20() + charVm.Actuals.Crafts.Medicine,
            _ => throw new Exception("Wrong craft provided.")
        };
    }
    #endregion
}
