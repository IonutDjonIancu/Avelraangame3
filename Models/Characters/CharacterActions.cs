namespace Models;

public class CharacterActions
{
    public Guid SourceId { get; set; }
    public Guid SessionId { get; set; }
    public Guid TargetId { get; set; }

    public Guid BoardId { get; set; }
    public string BoardType { get; set; }

    public string ActionType { get; set; }
}

public class CharacterLevelup
{
    public Guid CharacterId { get; set; }
    public Guid SessionId { get; set; }

    public string Stat { get; set; }
}
