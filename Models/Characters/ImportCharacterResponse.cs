namespace Models;

public class ImportCharacterResponse
{
    public Guid CharacterId { get; set; }
    public Guid SessionId { get; set; }
    public string CharacterName { get; set; }
}
