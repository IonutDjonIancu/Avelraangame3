namespace Models;

public class EquipItem
{
    public Guid ItemId { get; set; }
    public Guid CharacterId { get; set; }
    public Guid PlayerId { get; set; }
}
