using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.ViewModels;

namespace MushroomForum.Controllers
{
    public class MushroomHarvestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MushroomHarvestController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 8;

            var query = _context.MushroomHarvestEntries
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date);

            int totalEntries = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalEntries / (double)currentPageSize);

            var entries = await query
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToListAsync();

            var vm = new MushroomHarvestIndexViewModel
            {
                Entries = entries,
                PageNumber = currentPage,
                PageSize = currentPageSize,
                TotalPages = totalPages,
                TotalEntries = totalEntries
            };
            return View(vm);
        }
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new MushroomHarvestEntry());
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MushroomHarvestEntry entry, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                entry.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                entry.Date = entry.Date == default ? DateTime.Now : entry.Date;

                if (photo != null && photo.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                    var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    entry.PhotoUrl = "/uploads/" + fileName;
                }
                else
                {
                    entry.PhotoUrl = "/images/default-mushroom.jpg";
                }

                _context.MushroomHarvestEntries.Add(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entry);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _context.MushroomHarvestEntries.FindAsync(id);
            if (entry == null || entry.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return NotFound();
            return View(entry);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MushroomHarvestEntry entry, IFormFile? photo)
        {
            var dbEntry = await _context.MushroomHarvestEntries.FindAsync(entry.Id);
            if (dbEntry == null || dbEntry.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return NotFound();

            if (ModelState.IsValid)
            {
                dbEntry.MushroomType = entry.MushroomType;
                dbEntry.Quantity = entry.Quantity;
                dbEntry.Date = entry.Date;
                dbEntry.Place = entry.Place;

                if (photo != null && photo.Length > 0)
                {
                    if (!string.IsNullOrEmpty(dbEntry.PhotoUrl) && dbEntry.PhotoUrl != "/images/default-mushroom.jpg")
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, dbEntry.PhotoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                    var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    dbEntry.PhotoUrl = "/uploads/" + fileName;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entry);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.MushroomHarvestEntries.FindAsync(id);
            if (entry == null || entry.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return NotFound();

            if (!string.IsNullOrEmpty(entry.PhotoUrl) && entry.PhotoUrl != "/images/default-mushroom.jpg")
            {
                var filePath = Path.Combine(_env.WebRootPath, entry.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.MushroomHarvestEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        [HttpGet]
        public IActionResult Export()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportJson(DateTime? startDate, DateTime? endDate, bool includeStatistics = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

                var exportData = new Dictionary<string, object>
                {
                    ["exportInfo"] = new
                    {
                        exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        dateRange = new
                        {
                            startDate = startDate?.ToString("yyyy-MM-dd"),
                            endDate = endDate?.ToString("yyyy-MM-dd")
                        },
                        totalEntries = harvests.Count
                    },
                    ["harvests"] = harvests
                };

                if (includeStatistics && harvests.Any())
                {
                    var statistics = new
                    {
                        totalHarvests = harvests.Count,
                        totalQuantity = harvests.Sum(h => h.quantity),
                        uniqueMushroomTypes = harvests.Select(h => h.mushroomType).Distinct().Count(),
                        uniquePlaces = harvests.Select(h => h.place).Distinct().Count(),
                        mushroomTypeBreakdown = harvests
                            .GroupBy(h => h.mushroomType)
                            .Select(g => new
                            {
                                mushroomType = g.Key,
                                count = g.Count(),
                                totalQuantity = g.Sum(h => h.quantity)
                            })
                            .OrderByDescending(x => x.totalQuantity)
                            .ToList(),
                        placeBreakdown = harvests
                            .GroupBy(h => h.place)
                            .Select(g => new
                            {
                                place = g.Key,
                                count = g.Count(),
                                totalQuantity = g.Sum(h => h.quantity)
                            })
                            .OrderByDescending(x => x.totalQuantity)
                            .ToList()
                    };

                    exportData["statistics"] = statistics;
                }

                var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                var fileName = $"zbiory-grzybow-{DateTime.Now:yyyy-MM-dd-HHmmss}.json";
                var contentType = "application/json";

                return File(System.Text.Encoding.UTF8.GetBytes(json), contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Wystąpił błąd podczas eksportu danych: {ex.Message}";
                return RedirectToAction(nameof(Export));
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewJson(DateTime? startDate, DateTime? endDate, bool includeStatistics = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

                var exportData = new Dictionary<string, object>
                {
                    ["exportInfo"] = new
                    {
                        exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        dateRange = new
                        {
                            startDate = startDate?.ToString("yyyy-MM-dd"),
                            endDate = endDate?.ToString("yyyy-MM-dd")
                        },
                        totalEntries = harvests.Count
                    },
                    ["harvests"] = harvests
                };

                if (includeStatistics && harvests.Any())
                {
                    var statistics = new
                    {
                        totalHarvests = harvests.Count,
                        totalQuantity = harvests.Sum(h => h.quantity),
                        uniqueMushroomTypes = harvests.Select(h => h.mushroomType).Distinct().Count(),
                        uniquePlaces = harvests.Select(h => h.place).Distinct().Count(),
                        mushroomTypeBreakdown = harvests
                            .GroupBy(h => h.mushroomType)
                            .Select(g => new
                            {
                                mushroomType = g.Key,
                                count = g.Count(),
                                totalQuantity = g.Sum(h => h.quantity)
                            })
                            .OrderByDescending(x => x.totalQuantity)
                            .ToList(),
                        placeBreakdown = harvests
                            .GroupBy(h => h.place)
                            .Select(g => new
                            {
                                place = g.Key,
                                count = g.Count(),
                                totalQuantity = g.Sum(h => h.quantity)
                            })
                            .OrderByDescending(x => x.totalQuantity)
                            .ToList()
                    };

                    exportData["statistics"] = statistics;
                }

                var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                     Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                ViewBag.JsonData = json;
                ViewBag.ExportParams = new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    IncludeStatistics = includeStatistics
                };

                return View("ViewJson");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Wystąpił błąd podczas generowania danych: {ex.Message}";
                return RedirectToAction(nameof(Export));
            }
        }
    }
}