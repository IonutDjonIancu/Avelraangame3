namespace Models;

public class Market
{
    public Items Items { get; set; } = new();

    public Characters Characters { get; set; } = new();
}
