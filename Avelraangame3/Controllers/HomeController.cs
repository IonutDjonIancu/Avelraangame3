using Microsoft.AspNetCore.Mvc;
using Services;

namespace Avelraangame3.Controllers;

public class HomeController : Controller
{
    private readonly ICharacterService _characterService;

    public HomeController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    #region views
    // GET: Home/Index
    public IActionResult Index()
    {
        try
        {
            var charactersPortraits = _characterService.GetAllAliveCharacters();

            return View(charactersPortraits);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: Home/Error?info=infoToDisplay
    public IActionResult Error(string info)
    {
        return Content($"<<< click back to return\n\n\n{info}");
    }
    #endregion
}
