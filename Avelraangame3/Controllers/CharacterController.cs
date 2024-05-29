using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

public class CharacterController(ICharacterService characterService) : Controller
{
    private readonly ICharacterService _characterService = characterService;

    #region views
    // GET: Character/Upload
    public IActionResult Upload()
    {
        return View();
    }

    // GET: Character/Create
    public IActionResult Create()
    {
        return View();
    }

    // GET: Character/Details/5
    public IActionResult Details()
    {
        return View();
    }

    // GET: Character/Error?info=infoToDisplay
    public IActionResult Error(string info)
    {
        return Content(info);
    }
    #endregion

    #region requests
    [HttpGet]
    public IActionResult GetCharacter(string characterId, string sessionId)
    {
        try
        {
            var character = _characterService.GetCharacter(Guid.Parse(characterId), Guid.Parse(sessionId));

            return Ok(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // POST: Character/CreateCharacter
    [HttpPost]
    public IActionResult CreateCharacter([FromBody] CreateCharacter createCharacter)
    {
        try
        {
            var characterString = _characterService.CreateCharacter(createCharacter);

            return Ok(characterString);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: Character/CreateCharacter
    [HttpPost]
    public IActionResult ImportCharacter([FromBody] ImportCharacter characterString)
    {
        try
        {
            var characterResponse = _characterService.ImportCharacter(characterString);

            return Ok(characterResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion
}
