using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

public class ActionsController : Controller
{
    public IActionService _actionService { get; set; }

    public ActionsController(IActionService actionService)
    {
        _actionService = actionService;
    }

    #region views
    #endregion

    #region requests
    // PUT: Actions/RunAction
    [HttpPut]
    public IActionResult RunAction([FromBody] CharacterActions offense)
    {
        try
        {
            _actionService.RunCharacterActions(offense);

            return Ok();
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }


    #endregion

    #region private methods
    private ContentResult Error(string info)
    {
        return Content($"<<< click back to return\n\n\n{info}");
    }
    #endregion
}
