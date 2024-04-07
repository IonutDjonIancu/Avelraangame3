namespace Models;

public class CharacterDetails
{
    public string Name { get; set; }
    public int Entitylevel { get; set; }
    public int Levelup { get; set; }
    public bool IsHidden { get; set; }
    public bool IsAlive { get; set; }
    public bool IsLocked { get; set; }

    public int Wealth { get; set; }
}
