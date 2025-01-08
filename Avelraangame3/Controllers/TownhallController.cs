using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

[Route("Townhall")]
public class TownhallController : Controller
{
    private readonly ICharacterService _characterService;
    private readonly ITownhallService _townhallService;

    public TownhallController(
        ICharacterService characterService,
        ITownhallService townhallService)
    {
        _characterService = characterService;
        _townhallService = townhallService;
    }

    #region views
    // GET: ""
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    // GET: /Market/123
    [HttpGet("Market/{playerId}")]
    public IActionResult Market(Guid playerId)
    {
        try
        {
            var items = _townhallService.GetMarket(playerId);

            return View(items);
        }
        catch (Exception)
        {

            throw;
        }

    }

    // GET: Townhall/Duels/123
    [HttpGet("Duels/{playerId}")]
    public IActionResult Duels(Guid playerId)
    {
        try
        {
            var characters = _characterService.GetAllCharacters(playerId);

            return View(characters);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // GET: Character/Error?info=infoToDisplay
    public IActionResult Error(string info)
    {
        return Content($"<<< click the back button of the browser to return\n\n\n{info}");
    }
    #endregion

    #region requests
    
    #endregion
}
