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

    public IActionResult Index()
    {
        var charactersPortraits = _characterService.GetAllCharacters();

        return View(charactersPortraits);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
