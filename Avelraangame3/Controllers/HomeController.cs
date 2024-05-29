using Microsoft.AspNetCore.Mvc;

namespace Avelraangame3.Controllers;

public class HomeController() : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
