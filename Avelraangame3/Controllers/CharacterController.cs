using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Avelraangame3.Controllers
{
    public class CharacterController(
        IDiceService diceService,
        ICharacterService characterService) : Controller
    {
        private readonly IDiceService _diceService = diceService;
        private readonly ICharacterService _characterService = characterService;

        #region views
        // GET: Character/Index
        public IActionResult Index()
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
}
