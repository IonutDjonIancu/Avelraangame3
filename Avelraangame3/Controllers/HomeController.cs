using Avelraangame3.Models;
using Avelraangame3.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
