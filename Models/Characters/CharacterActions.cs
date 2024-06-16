namespace Models;

public class CharacterActions
{
}

public class CharacterLevelup
{
    public Guid CharacterId { get; set; }
    public Guid SessionId { get; set; }

    public string Attribute { get; set; }
}
