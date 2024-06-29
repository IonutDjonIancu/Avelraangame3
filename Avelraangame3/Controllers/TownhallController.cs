using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

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

    // GET: Character/Error?info=infoToDisplay
    public IActionResult Error(string info)
    {
        return Content($"<<< click back to return\n\n\n{info}");
    }
    #endregion

    #region requests
    

    #endregion
}
