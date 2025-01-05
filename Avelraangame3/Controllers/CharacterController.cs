using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers;

[Route("Character")]
public class CharacterController : Controller
{
    private readonly IValidatorService _validatorService;
    private readonly ICharacterService _characterService;

    public CharacterController(
        IValidatorService validatorService,
        ICharacterService characterService)
    {
        _validatorService = validatorService;
        _characterService = characterService;
    }

    #region views
    // GET: Character/Index
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    // GET: Character/Create
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // GET: Character/Details
    [HttpGet("Details")]
    public IActionResult Details()
    {
        return View();
    }

    // GET: Character//DetailsOf/123/456
    [HttpGet("DetailsOf/{playerId}/{characterId}")]
    public IActionResult DetailsOf(string playerId, string characterId)
    {
        try
        {
            _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");
            _validatorService.ValidateGuid(characterId, "Character id is in wrong format.");

            var character = _characterService.GetCharacter(new CharacterIdentity
            {
                Id = Guid.Parse(characterId),
                PlayerId = Guid.Parse(playerId)
            });

            return View(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion

    #region requests
    // GET: Character//Character/123/456
    [HttpGet("GetCharacter/{playerId}/{characterId}")]
    public IActionResult GetCharacter(string playerId, string characterId)
    {
        try
        {
            _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");
            _validatorService.ValidateGuid(characterId, "Character id is in wrong format.");

            var character = _characterService.GetCharacter(new CharacterIdentity
            {
                Id = Guid.Parse(characterId),
                PlayerId = Guid.Parse(playerId)
            });

            return Ok(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: Character/GetAllAliveCharacters/123
    [HttpGet("GetAllAliveCharacters/{playerId}")]
    public IActionResult GetAllAliveCharacters(string playerId)
    {
        try
        {
            _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");

            var character = _characterService.GetAllAliveCharacters(Guid.Parse(playerId));

            return Ok(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: Character/GetAllLockedCharacters/123
    [HttpGet("GetAllLockedCharacters/{playerId}")]
    public IActionResult GetAllLockedCharacters(string playerId)
    {
        try
        {
            _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");

            var character = _characterService.GetAllLockedCharacters(Guid.Parse(playerId));

            return Ok(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: Character/GetAllDuelistCharacters/123
    [HttpGet("GetAllDuelistCharacters/{playerId}")]
    public IActionResult GetAllDuelistCharacters(string playerId)
    {
        try
        {
            _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");

            var character = _characterService.GetAllDuelistCharacters(Guid.Parse(playerId));

            return Ok(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: Character/CreateCharacter
    [HttpPost]
    public IActionResult CreateCharacter([FromBody] CreateCharacter create)
    {
        try
        {
            _characterService.CreateCharacter(create);

            return Ok();
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
