using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

public class TownhallController : Controller
{
    private readonly ITownhallService _townhallService;

    public TownhallController(ITownhallService townhallService)
    {
        _townhallService = townhallService;
    }   

    #region views
    // GET: Townhall/Index
    public IActionResult Index()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // GET: Townhall/Duel
    public IActionResult Duel()
    {
        return View();
    }

    // GET: Townhall/DuelOf?characterId=string&sessionId=string
    public IActionResult DuelOf(Guid characterId, Guid sessionId)
    {
        try
        {
            var characterDuel = _townhallService.GetOrGenerateDuel(new CharacterIdentity
            {
                Id = characterId,
                SessionId = sessionId
            });

            return View(characterDuel);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion
    #region requests

    #endregion

    #region private methods
    private ContentResult Error(string info)
    {
        return Content($"<<< click back to return\n\n\n{info}");
    }
    #endregion
}
