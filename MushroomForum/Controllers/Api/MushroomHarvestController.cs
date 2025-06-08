using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;

namespace MushroomForum.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MushroomHarvestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MushroomHarvestController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserHarvests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Użytkownik nie jest zalogowany" });
            }

            try
            {
                var harvests = await _context.MushroomHarvestEntries
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.Date)
                    .Select(e => new
                    {
                        id = e.Id,
                        mushroomType = e.MushroomType,
                        quantity = e.Quantity,
                        date = e.Date.ToString("yyyy-MM-dd"),
                        place = e.Place,
                        photoUrl = e.PhotoUrl
                    })
                    .ToListAsync();

                var result = new
                {
                    exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalEntries = harvests.Count,
                    harvests = harvests
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Wystąpił błąd podczas pobierania danych", error = ex.Message });
            }
        }

        [HttpGet("daterange")]
        public async Task<IActionResult> GetUserHarvestsByDateRange([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Użytkownik nie jest zalogowany" });
            }

            try
            {
                var query = _context.MushroomHarvestEntries
                    .Where(e => e.UserId == userId);

                if (startDate.HasValue)
                {
                    query = query.Where(e => e.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(e => e.Date <= endDate.Value);
                }

                var harvests = await query
                    .OrderByDescending(e => e.Date)
                    .Select(e => new
                    {
                        id = e.Id,
                        mushroomType = e.MushroomType,
                        quantity = e.Quantity,
                        date = e.Date.ToString("yyyy-MM-dd"),
                        place = e.Place,
                        photoUrl = e.PhotoUrl
                    })
                    .ToListAsync();

                var result = new
                {
                    exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    dateRange = new
                    {
                        startDate = startDate?.ToString("yyyy-MM-dd"),
                        endDate = endDate?.ToString("yyyy-MM-dd")
                    },
                    totalEntries = harvests.Count,
                    harvests = harvests
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Wystąpił błąd podczas pobierania danych", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserHarvestStatistics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Użytkownik nie jest zalogowany" });
            }

            try
            {
                var harvests = await _context.MushroomHarvestEntries
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

                var statistics = new
                {
                    totalHarvests = harvests.Count,
                    totalQuantity = harvests.Sum(h => h.Quantity),
                    uniqueMushroomTypes = harvests.Select(h => h.MushroomType).Distinct().Count(),
                    uniquePlaces = harvests.Select(h => h.Place).Distinct().Count(),
                    firstHarvestDate = harvests.Any() ? harvests.Min(h => h.Date).ToString("yyyy-MM-dd") : null,
                    lastHarvestDate = harvests.Any() ? harvests.Max(h => h.Date).ToString("yyyy-MM-dd") : null,
                    mushroomTypeBreakdown = harvests
                        .GroupBy(h => h.MushroomType)
                        .Select(g => new
                        {
                            mushroomType = g.Key,
                            count = g.Count(),
                            totalQuantity = g.Sum(h => h.Quantity)
                        })
                        .OrderByDescending(x => x.totalQuantity)
                        .ToList(),
                    placeBreakdown = harvests
                        .GroupBy(h => h.Place)
                        .Select(g => new
                        {
                            place = g.Key,
                            count = g.Count(),
                            totalQuantity = g.Sum(h => h.Quantity)
                        })
                        .OrderByDescending(x => x.totalQuantity)
                        .ToList()
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Wystąpił błąd podczas pobierania statystyk", error = ex.Message });
            }
        }
    }
}