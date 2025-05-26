using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;

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

    }
}

