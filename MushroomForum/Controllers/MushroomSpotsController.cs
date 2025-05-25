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
        public async Task<IActionResult> GetAll()
        {
            var spots = await _context.MushroomSpots.ToListAsync();
            return Json(spots);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
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
