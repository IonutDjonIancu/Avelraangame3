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
        var create = new CreateCharacter
        {
            Name = name,
            Race = Statics.Races.Human,
            Culture = Statics.Cultures.Danarian,
            Spec = Statics.Specs.Warring
        };

        // act
        var chrString = _characterService.CreateCharacter(create);
        var chrDecr = EncryptionService.DecryptString(chrString);
        var character = JsonConvert.DeserializeObject<Character>(chrDecr);

        // assert
        character.Should().NotBeNull();
        character!.Details.Name.Should().Be(name);
        _snapshot.Characters.Exists(c => c.Identity.Id == character.Identity.Id).Should().BeTrue();
    }
}