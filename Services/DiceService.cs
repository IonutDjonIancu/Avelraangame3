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

    double Roll_vs_effort(CharacterVm charVm, string craft, int effort, ISnapshot snapshot);
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
    public double Roll_vs_effort(CharacterVm charVm, string feat, int effort, ISnapshot snapshot)
    {
        if (!Statics.Feats.All.Contains(feat))
            throw new Exception("No such craft found to roll.");

        Validators.ValidateAgainstNull(charVm, "CharacterVm cannot be null.");

        var charRoll = RollFeat(charVm, feat, snapshot);
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
    private int RollFeat(CharacterVm charVm, string craft, ISnapshot snapshot)
    {
        var character = snapshot.Characters.Find(s => s.Identity.Id == charVm.Identity.Id)!;
        var roll = Roll_d20();

        UpgradeEntityLevel(roll, character);
        LevelUp(roll, character);

        return craft switch
        {
            Statics.Feats.Combat => Roll_d20() + charVm.Actuals.Feats.Combat,
            Statics.Feats.Strength => Roll_d20() + charVm.Actuals.Feats.Strength,
            Statics.Feats.Tactics => Roll_d20() + charVm.Actuals.Feats.Tactics,
            Statics.Feats.Athletics => Roll_d20() + charVm.Actuals.Feats.Athletics,
            Statics.Feats.Survival => Roll_d20() + charVm.Actuals.Feats.Survival,
            Statics.Feats.Social => Roll_d20() + charVm.Actuals.Feats.Social,
            Statics.Feats.Abstract => Roll_d20() + charVm.Actuals.Feats.Abstract,
            Statics.Feats.Psionic => Roll_d20() + charVm.Actuals.Feats.Psionic,
            Statics.Feats.Crafting => Roll_d20() + charVm.Actuals.Feats.Crafting,
            Statics.Feats.Medicine => Roll_d20() + charVm.Actuals.Feats.Medicine,
            _ => throw new Exception("Wrong craft provided.")
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
