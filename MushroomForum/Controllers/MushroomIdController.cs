using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MushroomForum.Models;
using MushroomForum.Services;
using MushroomForum.ViewModels;

namespace MushroomForum.Controllers
{
    public class MushroomIdController : Controller
    {
        private readonly MushroomIdService _mushroomIdService;

        public MushroomIdController(MushroomIdService mushroomIdService)
        {
            _mushroomIdService = mushroomIdService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new MushroomIdResultViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IFormFile image)
        {
            var model = new MushroomIdResultViewModel();
            if (image == null || image.Length == 0)
            {
                model.Success = false;
                model.ErrorMessage = "Wybierz zdjęcie do identyfikacji";
                return View(model);
            }

            model = await _mushroomIdService.IdentifyAsync(image);
            return View(model);
        }
    }
}
