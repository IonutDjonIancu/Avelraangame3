namespace Models;

public class CreateCharacter
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; }
    public string Portrait { get; set; }
    public string Race { get; set; }
    public string Culture { get; set; }
    public string Spec { get; set; }

}
