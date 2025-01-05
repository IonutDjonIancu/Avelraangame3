using Microsoft.Extensions.DependencyInjection;
using Models;
using Services;

namespace Tests;

public class TestBase : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IServiceProvider _provider;

    public readonly ISnapshot _snapshot;
    public readonly IDiceService _diceService;
    public readonly IItemService _itemService;
    public readonly ICharacterService _characterService;


    public TestBase()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();

        _snapshot = _provider.GetRequiredService<ISnapshot>();
        _diceService = _provider.GetRequiredService<IDiceService>();
        _itemService = _provider.GetRequiredService<IItemService>();
        _characterService = _provider.GetRequiredService<ICharacterService>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddSingleton<ISnapshot, Snapshot>();
        services.AddSingleton<IDiceService, DiceService>();
        services.AddTransient<IItemService, ItemService>();
        services.AddTransient<ICharacterService, CharacterService>();
    }
}
