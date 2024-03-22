using Avelraangame3.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Avelraangame3.Controllers
{
    public class CharacterController(ISnapshotService snapshotService) : Controller
    {
        private readonly Snapshot snapshot = snapshotService.Snapshot;

        // GET: CharacterController
        public IActionResult Index()
        {
            return View();
        }

        // GET: CharacterController/Details/5
        public IActionResult Details(int id)
        {
            return View();
        }

        // GET: CharacterController/Create
        public IActionResult Create()
        {
            return View();
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
    }
}
