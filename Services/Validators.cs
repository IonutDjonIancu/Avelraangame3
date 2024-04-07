using Models;

namespace Services;

internal class Validators
{
    #region general
    internal static void ValidateAgainstNull(object obj, string message)
    {
        if (obj == null)
            throw new Exception(message);
    }

    internal static void ValidateString(string str, string message)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new Exception(message);
    }
    #endregion

    #region character
    internal static void ValidateOnGetCharacter(Guid id, Guid sessionId)
    {
        if (id == Guid.Empty || sessionId == Guid.Empty)
            throw new Exception("Id or SessionId are either missing or invalid.");
    }

    internal static void ValidateOnCreateCharacter(CreateCharacter character)
    {
        ValidateAgainstNull(character, "CreateCharacter is null");
        ValidateString(character.Name, "Name cannot be empty.");
        ValidateString(character.Race, "Race cannot be empty.");
        ValidateString(character.Culture, "Culture cannot be empty.");
        ValidateString(character.Spec, "Spec cannot be empty.");
        
        if (character.Name.Length > 30) throw new Exception("Name too long, 30 characters max.");
        if (!Statics.Races.All.Contains(character.Race.ToLower())) throw new Exception("Race not found.");
        if (!Statics.Cultures.All.Contains(character.Culture.ToLower())) throw new Exception("Culture not found.");
        if (!Statics.Specs.All.Contains(character.Spec.ToLower())) throw new Exception("Specialization not found.");
    }

    #endregion

}
