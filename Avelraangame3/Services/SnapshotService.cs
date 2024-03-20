using Avelraangame3.Models;

namespace Avelraangame3.Services;

public interface ISnapshotService
{
    Snapshot Snapshot { get; set; }
}

public class SnapshotService : ISnapshotService
{
    public Snapshot Snapshot { get; set; }

    public SnapshotService()
    {
        Snapshot = new Snapshot();

        AddDefaultCharacter();
    }

    private void AddDefaultCharacter()
    {
        var defaultChar = new Character
        {
            Id = Guid.Empty,
            Name = "Joe Doe"
        };

        Snapshot.Characters.Add(defaultChar);
    }

}
