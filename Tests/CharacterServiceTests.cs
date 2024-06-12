using FluentAssertions;
using Models;
using Newtonsoft.Json;
using Services;

namespace Tests;

public class CharacterServiceTests : TestBase
{
    [Fact]
    public void Create_character_should_return_a_hashed_string()
    {
        // arrange
        var name = "asda";
        var portrait = "https://i.pinimg.com/564x/74/3d/a5/743da528a04dd29198cb2f3c46a8c84c.jpg";
        var create = new CreateCharacter
        {
            Name = name,
            Portrait = portrait,
            Race = Statics.Races.Elf,
            Culture = Statics.Cultures.Highborn,
            Spec = Statics.Specs.Sorcery
        };

        // act
        var chrString = _characterService.CreateCharacter(create);
        var chrDecr = EncryptionService.DecryptString(chrString);
        var character = JsonConvert.DeserializeObject<Character>(chrDecr);

        // assert
        character.Should().NotBeNull();
        character!.Details.Name.Should().Be(name);
        _snapshot.Characters.FirstOrDefault(c => c.Identity.Id == character.Identity.Id).Should().NotBeNull();
    }
}