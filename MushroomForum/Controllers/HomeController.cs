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
                "Grzyby to organizmy, które nie s¹ ani roœlinami, ani zwierzêtami!",
                "W Japonii grzyby shiitake uwa¿ane s¹ za symbol d³ugowiecznoœci.",
                "Niektóre grzyby maj¹ zdolnoœæ bioluminescencji, œwiec¹ w ciemnoœci!",
                "Grzyby truj¹ce mog¹ wygl¹daæ podobnie do grzybów jadalnych, wiêc zawsze upewnij siê, ¿e masz pewnoœæ co do ich identyfikacji!"
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
