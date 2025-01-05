using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

[Route("/")]
public class HomeController : Controller
{
    private readonly ISnapshot _snapshot;
    private readonly IValidatorService _validatorService;

    public HomeController(
        ISnapshot snapshot,
        IValidatorService validatorService)
    {
        _snapshot = snapshot;
        _validatorService = validatorService;
    }

    #region views
    // GET: Home/Index
    public IActionResult Index()
    {
        return View();
    }

    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: Home/Error?info=infoToDisplay
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
            _validatorService.ValidateOnCreatePlayer(playerName);

            var playerId = Guid.NewGuid();

            var player = new Player
            {
                Name = playerName,
                Id = playerId
            };

            _snapshot.Players.Add(player);
            
            return Ok(player);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    #endregion
}
