using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

public class TownhallController : Controller
{
    private readonly ICharacterService _characterService;
    private readonly INpcService _npcService;

    public TownhallController(
        ICharacterService characterService,
        INpcService npcService)
    {
        _characterService = characterService;
        _npcService = npcService;
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

    // GET: Townhall/Duel?characterId=string&sessionId=string
    public IActionResult Duel(string characterId, string sessionId)
    {
        try
        {
            var character = _characterService.GetCharacter(new CharacterIdentity
            {
                Id = Guid.Parse(characterId),
                PlayerId = Guid.Parse(sessionId)
            });

            return View(character);
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
