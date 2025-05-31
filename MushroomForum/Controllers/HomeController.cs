using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MushroomForum.Models;
using MushroomForum.Services;

namespace MushroomForum.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CiekawostkiService _ciekawostkiService;

        public HomeController(ILogger<HomeController> logger, CiekawostkiService ciekawostkiService)
        {
            _logger = logger;
            _ciekawostkiService = ciekawostkiService;
        }

        public IActionResult Index()
        {
            var ciekawostkaNaDzien = _ciekawostkiService.GetCiekawostkaNaDzien();

            var memy = new List<string>
            {
                "/images/mem1.jpg",
                "/images/mem2.jpg",
                "/images/mem3.jpg",
                "/images/mem4.jpg"
            };


            var random = new Random();
            var losowyMem = memy[random.Next(memy.Count)];

            ViewData["Ciekawostka"] = ciekawostkaNaDzien;
            ViewData["Mem"] = losowyMem;
            return View();
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
