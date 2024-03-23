namespace Services;

public interface IDiceService
{
    int Roll_d20_withReroll(int total = 0);
}

public class DiceService : IDiceService
{
    static readonly Random random = new();

    public int Roll_d20_withReroll(int total = 0)
    {
        total += random.Next(1, 21);

        if (total % 20 == 0)
        {
            return Roll_d20_withReroll(total);
        }

        return total;
    }
}
