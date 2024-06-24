using Microsoft.AspNetCore.Mvc;
using Services;

namespace Avelraangame3.Controllers;

public class TestController(
    IDiceService diceService) : Controller
{
    private readonly IDiceService _diceService = diceService;

    public IActionResult Roll20()
    {
        return Ok(_diceService.Roll_d20());
    }
}
