using Avelraangame3.Persistence;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Avelraangame3.Controllers
{
    public class CharacterController(
        ISnapshotService snapshotService,
        IDiceService diceService) : Controller
    {
        private readonly Snapshot snapshot = snapshotService.Snapshot;
        private readonly IDiceService diceService = diceService;

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
        public IActionResult Details(string id, string sessionId)
        {
            var character = snapshot.Characters.Find(s => s.Identity.Id == id && s.Identity.SessionId == sessionId);

            return View(character);
        }


        // POST: CharacterController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection collection)
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

        // GET: CharacterController/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }

        // POST: CharacterController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            return diceService.Roll_d20_withReroll();
        }
    }
}
