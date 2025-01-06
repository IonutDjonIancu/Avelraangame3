using Microsoft.AspNetCore.Mvc;
using Services;

namespace Avelraangame3.Controllers;

[Route("Item")]
public class ItemController : Controller
{
    private readonly IItemService _itemService;

    public ItemController(IItemService itemService)
    {
        _itemService = itemService;
    }

    #region requests
    [HttpGet("GetRandomItem")]
    public IActionResult GetRandomItem()
    {
        return Ok();
    }
    #endregion
}
