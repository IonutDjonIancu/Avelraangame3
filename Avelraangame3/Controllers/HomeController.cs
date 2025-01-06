using Microsoft.AspNetCore.Mvc;
using Services;

namespace Avelraangame3.Controllers;

[Route("")]
public class HomeController : Controller
{
    private readonly IPlayerService _playerService;

    public HomeController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    #region views
    // GET: /
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    // GET: Privacy
    [HttpGet("Privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: Error?info=infoToDisplay
    [HttpGet("Error")]
    public IActionResult Error(string info)
    {
        return Content($"<<< click the back button of the browser to return\n\n\n{info}");
    }
    #endregion

    #region requests
    [HttpPost("CreatePlayer/{playerName}")]
    public IActionResult CreatePlayer(string playerName)
    {
        try
        {
            var player = _playerService.CreatePlayer(playerName);
            
            return Ok(player);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion
}
