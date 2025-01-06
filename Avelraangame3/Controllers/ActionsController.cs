using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

[Route("Actions")]
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
    public IActionResult RunAction([FromBody] CharacterActions actions)
    {
        try
        {
            _actionService.RunActionLogic(actions);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    #endregion
}
