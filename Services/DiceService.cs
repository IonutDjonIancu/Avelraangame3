namespace Services;

public interface IDiceService
{
    /// <summary>
    /// With reroll.
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    int Roll_d20_rr(int total = 0);

    /// <summary>
    /// Does not have reroll.
    /// </summary>
    /// <returns></returns>
    int Roll_d20();

    /// <summary>
    /// Does not have reroll.
    /// </summary>
    /// <returns></returns>
    int Roll_1dn(int n);

    /// <summary>
    /// Does not have reroll.
    /// </summary>
    /// <returns></returns>
    int Roll_mdn(int m, int n);
}

public class DiceService : IDiceService
{
    static readonly Random random = new();

    public int Roll_1dn(int n)
    {
        return random.Next(1, n + 1);   
    }

    public int Roll_mdn(int m, int n)
    {
        Validators.ValidateDiceMdNRoll(m, n);

        return random.Next(m, n + 1);
    }

    public int Roll_d20()
    {
        return random.Next(1, 21);
    }

    public int Roll_d20_rr(int total = 0)
    {
        total += random.Next(1, 21);

        if (total % 20 == 0)
        {
            return Roll_d20_rr(total);
        }

        return total;
    }
}
