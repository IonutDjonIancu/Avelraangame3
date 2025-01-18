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

    // GET: Character/Details/123
    [HttpGet("Details/{playerId}")]
    public IActionResult Details(string playerId)
    {
        _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");

        var character = _characterService.GetAllCharactersByPlayerId(Guid.Parse(playerId));

        return View(character);
    }

    // GET: Character//DetailsOf/123/456
    [HttpGet("DetailsOf/{playerId}/{characterId}")]
    public IActionResult DetailsOf(string playerId, string characterId)
    {
        try
        {
            _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");
            _validatorService.ValidateGuid(characterId, "Character id is in wrong format.");

            var character = _characterService.GetCharacterByPlayerId(new CharacterIdentity
            {
                CharacterId = Guid.Parse(characterId),
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

            var character = _characterService.GetCharacterByPlayerId(new CharacterIdentity
            {
                CharacterId = Guid.Parse(characterId),
                PlayerId = Guid.Parse(playerId)
            });

            return Ok(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: Character/GetAllCharacters/123
    [HttpGet("GetAllCharacters/{playerId}")]
    public IActionResult GetAllCharacters(string playerId)
    {
        try
        {
            _validatorService.ValidateGuid(playerId, "Player id is in wrong format.");

            var character = _characterService.GetAllCharactersByPlayerId(Guid.Parse(playerId));

            return Ok(character);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: Character/CreateCharacter
    [HttpPost("CreateCharacter")]
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
    [HttpPut("EquipItem")]
    public IActionResult EquipItem([FromBody] EquipItem item)
    {
        try
        {
            _characterService.EquipItem(item);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/UnequipItem
    [HttpPut("UnequipItem")]
    public IActionResult UnequipItem([FromBody] EquipItem item)
    {
        try
        {
            _characterService.UnequipItem(item);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/UnequipItem
    [HttpPut("SellItem")]
    public IActionResult SellItem([FromBody] EquipItem item)
    {
        try
        {
            _characterService.SellItem(item);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/BuyItemFromTown
    [HttpPut("BuyItemFromTown")]
    public IActionResult BuyItemFromTown([FromBody] EquipItem item)
    {
        try
        {
            _characterService.BuyItemFromTown(item);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: Character/Levelup
    [HttpPut("Levelup")]
    public IActionResult Levelup([FromBody] CharacterLevelup levelup)
    {
        try
        {
            _characterService.Levelup(levelup);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion
}
