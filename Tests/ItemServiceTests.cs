using FluentAssertions;

namespace Tests;

public class ItemServiceTests : TestBase
{
    [Fact]
    public void Create_a_lot_of_items()
    {
        // act
        var items = _itemService.GenerateRandomItems(100);

        // assert
       items.Should().NotBeNullOrEmpty();
        // such test much wow
    }
}
