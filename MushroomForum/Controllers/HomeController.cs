using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MushroomForum.Models;

namespace MushroomForum.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var ciekawostki = new List<string>
            {
                "Grzyby to organizmy, kt�re nie s� ani ro�linami, ani zwierz�tami!",
                "W Japonii grzyby shiitake uwa�ane s� za symbol d�ugowieczno�ci.",
                "Niekt�re grzyby maj� zdolno�� bioluminescencji, �wiec� w ciemno�ci!",
                "Grzyby truj�ce mog� wygl�da� podobnie do grzyb�w jadalnych, wi�c zawsze upewnij si�, �e masz pewno�� co do ich identyfikacji!"
            };

            var random = new Random();
            var losowaCiekawostka = ciekawostki[random.Next(ciekawostki.Count)];

            var memy = new List<string>
            {
                "/images/mem1.jpg",
                "/images/mem2.jpg",
                "/images/mem3.jpg",
                "/images/mem4.jpg"
            };

            var losowyMem = memy[random.Next(memy.Count)];
            ViewData["Ciekawostka"] = losowaCiekawostka;
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
