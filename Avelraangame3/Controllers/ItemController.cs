using Microsoft.AspNetCore.Mvc;

namespace Avelraangame3.Controllers;

public class ItemController : Controller
{
    #region requests
    public IActionResult GetRandomItem()
    {


        return Ok();
    }
    #endregion
}
