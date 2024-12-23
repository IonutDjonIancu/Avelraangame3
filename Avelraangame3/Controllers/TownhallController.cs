using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

public class TownhallController : Controller
{
    private readonly ITownhallService _townhallService;
    private readonly ICharacterService _characterService;

    public TownhallController(
        ITownhallService townhallService,
        ICharacterService characterService)
    {
        _townhallService = townhallService;
        _characterService = characterService;
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
            return BadRequest(ex.Message);
        }
    }

    // GET: Townhall/Duel
    public IActionResult Duel()
    {
        var characters = _characterService.GetAllDuelistCharacters();

        return View(characters);
    }

    // GET: Townhall/DuelOf?characterId=string&sessionId=string
    public IActionResult DuelOf(Guid characterId, Guid sessionId, string effortLevelName)
    {
        try
        {
            var characterDuel = _townhallService.GenerateDuelVsNpc(new CharacterIdentity
            {
                Id = characterId,
                SessionId = sessionId
            }, effortLevelName);

            return View(characterDuel);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion
    #region requests

    #endregion
}
