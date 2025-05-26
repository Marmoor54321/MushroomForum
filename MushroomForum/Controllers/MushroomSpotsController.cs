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

namespace MushroomForum.Controllers
{
    [Authorize]
    public class MushroomSpotsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MushroomSpotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var spots = await _context.MushroomSpots.ToListAsync();
            return View(spots);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MushroomSpot spot)
        {
            if (ModelState.IsValid)
            {
                _context.MushroomSpots.Add(spot);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var spots = _context.MushroomSpots
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

