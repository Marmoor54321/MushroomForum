using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.ViewModels;
using QuestPDF.Helpers;

namespace MushroomForum.Controllers
{
    public class MushroomWikiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MushroomWikiController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentPage = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 8;

            var query = _context.MushroomWikiEntries
                .OrderByDescending(e => e.Date);

            int totalEntries = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalEntries / (double)currentPageSize);

            var entries = await query
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToListAsync();

            var vm = new MushroomWikiIndexViewModel
            {
                Entries = entries,
                PageNumber = currentPage,
                PageSize = currentPageSize,
                TotalPages = totalPages,
                TotalEntries = totalEntries
            };

            return View(vm);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new MushroomWikiEntry());
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MushroomWikiEntry entry, IFormFile? photo)
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
                    entry.PhotoUrl = "/images/default-wiki-mushroom.jpg";
                }

                _context.MushroomWikiEntries.Add(entry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entry);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _context.MushroomWikiEntries.FindAsync(id);
            if (entry == null || entry.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return NotFound();
            return View(entry);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MushroomWikiEntry entry, IFormFile? photo)
        {
            var dbEntry = await _context.MushroomWikiEntries.FindAsync(entry.Id);
            if (dbEntry == null || dbEntry.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return NotFound();

            if (ModelState.IsValid)
            {
                dbEntry.Name = entry.Name;
                dbEntry.LatinName = entry.LatinName;
                dbEntry.Description = entry.Description;
                dbEntry.Type = entry.Type;
                dbEntry.Date = entry.Date;
                

                if (photo != null && photo.Length > 0)
                {
                    if (!string.IsNullOrEmpty(dbEntry.PhotoUrl) && dbEntry.PhotoUrl != "/images/default-wiki-mushroom.jpg")
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
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.MushroomWikiEntries.FindAsync(id);
            if (entry == null || entry.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return NotFound();

            if (!string.IsNullOrEmpty(entry.PhotoUrl) && entry.PhotoUrl != "/images/default-wiki-mushroom.jpg")
            {
                var filePath = Path.Combine(_env.WebRootPath, entry.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.MushroomWikiEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
