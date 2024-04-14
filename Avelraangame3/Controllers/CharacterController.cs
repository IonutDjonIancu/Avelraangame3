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
        public IActionResult Details(Guid id, Guid sessionId)
        {
            try
            {
                var character = _characterService.GetCharacter(id, sessionId);

                return View(character);
            }
            catch (Exception ex)
            {
                return BadRequest($"Create character failed with exception: {ex.Message}");
            }

        }
        #endregion

        #region requests
        // POST: Character/CreateCharacter
        [HttpPost]
        public IActionResult CreateCharacter([FromBody] CreateCharacter createCharacter)
        {
            try
            {
                var character = _characterService.CreateCharacter(createCharacter);

                return Ok(character);
            }
            catch (Exception ex)
            {
                return BadRequest($"Create character failed with exception: {ex.Message}");
            }
        }

        // GET: CharacterController/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }

        // POST: CharacterController/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CharacterController/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: CharacterController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Character/Rolld20
        [HttpGet]
        public int Rolld20()
        {
            return _diceService.Roll_d20_rr();
        }
        #endregion
    }
}
