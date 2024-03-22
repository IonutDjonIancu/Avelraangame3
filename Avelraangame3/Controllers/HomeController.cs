using Avelraangame3.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Avelraangame3.Controllers
{
    public class HomeController(ISnapshotService snapshotService) : Controller
    {
        private readonly Snapshot snapshot = snapshotService.Snapshot;

        public IActionResult Index()
        {
            var firstChar = snapshot.Characters.FirstOrDefault();

            return View(firstChar);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
