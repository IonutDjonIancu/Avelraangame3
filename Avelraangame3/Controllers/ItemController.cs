using Microsoft.AspNetCore.Mvc;
using Services;

namespace Avelraangame3.Controllers;

public class ItemController : Controller
{
    private readonly IItemService _itemService;

    public ItemController(IItemService itemService)
    {
        _itemService = itemService;
    }

    #region requests
    public IActionResult GetRandomItem()
    {
        return Ok();
    }
    #endregion
}
