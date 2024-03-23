using Models;

namespace Avelraangame3.Persistence;

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
            Identity = new CharacterIdentity
            {
                Id = Guid.Empty.ToString(),
                Name = "Joe Doe"
            }
        };

        Snapshot.Characters.Add(defaultChar);
    }

}
