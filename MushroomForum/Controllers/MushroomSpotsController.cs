using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System;
using QuestPDF.Helpers;
using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Controllers
{
    [Authorize]
    public class MushroomSpotsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MushroomSpotsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var spots = await _context.MushroomSpots
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return View(spots);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MushroomSpot spot)
        {
            if (ModelState.IsValid)
            {
                spot.UserId = _userManager.GetUserId(User);
                _context.MushroomSpots.Add(spot);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            var userId = _userManager.GetUserId(User);
            var spots = _context.MushroomSpots
                .Where(s => s.UserId == userId)
                .Select(s => new {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Latitude,
                    s.Longitude,
                    s.Rating 
                })
                .ToList();

            return Json(spots);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit([FromBody] MushroomSpot updatedSpot)
        {
            var spot = await _context.MushroomSpots.FindAsync(updatedSpot.Id);
            if (spot == null)
                return NotFound();
            var userId = _userManager.GetUserId(User);
            if (spot.UserId != userId)
                return Forbid();
            spot.Name = updatedSpot.Name;
            spot.Description = updatedSpot.Description;
            spot.Rating = updatedSpot.Rating;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] 
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            var spot = await _context.MushroomSpots.FindAsync(id);
            if (spot == null)
                return NotFound();
            var userId = _userManager.GetUserId(User);
            if (spot.UserId != userId)
                return Forbid();
            _context.MushroomSpots.Remove(spot);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        [Authorize] 
        [IgnoreAntiforgeryToken]
        public IActionResult ExportToPdf([FromBody] ImageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Base64Image))
                return BadRequest("Brak obrazu");

            try
            {
                // usuń nagłówek "data:image/png;base64,"
                var base64 = request.Base64Image.Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(base64);

                var pdfBytes = CreatePdf(imageBytes);
                return File(pdfBytes, "application/pdf", "mapa_grzybobranie.pdf");
            }
            catch
            {
                return StatusCode(500, "Błąd generowania PDF");
            }
        }

        private byte[] CreatePdf(byte[] imageBytes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(16));

                    page.Content()
                        .Image(imageBytes);
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        public class ImageRequest
        {
            public string Base64Image { get; set; }
        }

    }
}

