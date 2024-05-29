namespace Models;

public class ImportCharacter
{
    public string CharacterString { get; set; }
}

public class ImportCharacterResponse
{
    public Guid CharacterId { get; set; }
    public Guid SessionId { get; set; }
}
