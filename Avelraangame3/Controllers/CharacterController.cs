using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

public class CharacterController(ICharacterService characterService) : Controller
{
    private readonly ICharacterService _characterService = characterService;

    #region views
    // GET: Character/Index
    public IActionResult Index()
    {
        return View();
    }

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

    // GET: Character/DetailsOf?characterId=string&sessionId=string
    public IActionResult DetailsOf(string characterId, string sessionId)
    {
        try
        {
            var character = _characterService.GetCharacter(new CharacterIdentity
            {
                Id = Guid.Parse(characterId),
                SessionId = Guid.Parse(sessionId)
            });

            return View(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: Character/Error?info=infoToDisplay
    #endregion

    #region requests
    // GET: Character/GetCharacter
    [HttpGet]
    public IActionResult GetCharacter(string characterId, string sessionId)
    {
        try
        {
            var character = _characterService.GetCharacter(new CharacterIdentity
            {
                Id = Guid.Parse(characterId),
                SessionId = Guid.Parse(sessionId)
            });

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

    // POST: Character/ImportCharacter
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

    // PUT: Character/ExportCharacter
    [HttpPut]
    public IActionResult ExportCharacter([FromBody] CharacterIdentity identity)
    {
        try
        {
            var characterResponse = _characterService.ExportCharacter(identity);

            return Ok(characterResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/EquipItem
    [HttpPut]
    public IActionResult EquipItem([FromBody] EquipItem equipItem)
    {
        try
        {
            var characterResponse = _characterService.EquipItem(equipItem);

            return Ok(characterResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/UnequipItem
    [HttpPut]
    public IActionResult UnequipItem([FromBody] EquipItem equipItem)
    {
        try
        {
            var characterResponse = _characterService.UnequipItem(equipItem);

            return Ok(characterResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/UnequipItem
    [HttpPut]
    public IActionResult SellItem([FromBody] EquipItem equipItem)
    {
        try
        {
            var characterResponse = _characterService.SellItem(equipItem);

            return Ok(characterResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/Levelup
    [HttpPut]
    public IActionResult Levelup([FromBody] CharacterLevelup levelup)
    {
        try
        {
            var characterResponse = _characterService.Levelup(levelup);

            return Ok(characterResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion
}
