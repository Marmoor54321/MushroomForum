using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using MushroomForum.Migrations;

namespace MushroomForum.Controllers
{
    public class MushroomNotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public MushroomNotesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: MushroomNotes
        public async Task<IActionResult> Index()
        {
            return View(await _context.MushroomNotes.ToListAsync());
        }

        // GET: MushroomNotes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mushroomNotes = await _context.MushroomNotes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mushroomNotes == null)
            {
                return NotFound();
            }

            return View(mushroomNotes);
        }

        // GET: MushroomNotes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MushroomNotes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MushroomNotes mushroomNotes, IFormFile PhotoUrl)
        {
            ModelState.Remove("PhotoUrl");

            mushroomNotes.CreateDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                if (PhotoUrl != null && PhotoUrl.Length > 0)
                {
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoUrl.FileName);
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PhotoUrl.CopyToAsync(stream);
                    }

                    mushroomNotes.PhotoUrl = "/uploads/" + fileName;
                }

                _context.Add(mushroomNotes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(mushroomNotes);
        }



        // GET: MushroomNotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mushroomNotes = await _context.MushroomNotes.FindAsync(id);
            if (mushroomNotes == null)
            {
                return NotFound();
            }
            return View(mushroomNotes);
        }

        // POST: MushroomNotes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MushroomNotes updatedNote, IFormFile? newPhoto)
        {
            if (!ModelState.IsValid)
                return View(updatedNote);

            var existingNote = await _context.MushroomNotes.FindAsync(id);
            if (existingNote == null)
                return NotFound();

            // Zaktualizuj tylko edytowalne pola
            existingNote.Title = updatedNote.Title;
            existingNote.Content = updatedNote.Content;

            // Obsługa nowego zdjęcia, jeśli użytkownik je dodał
            if (newPhoto != null && newPhoto.Length > 0)
            {
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newPhoto.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await newPhoto.CopyToAsync(stream);
                }

                // Opcjonalnie: usuń stare zdjęcie z serwera
                if (!string.IsNullOrEmpty(existingNote.PhotoUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingNote.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                existingNote.PhotoUrl = "/uploads/" + fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // GET: MushroomNotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mushroomNotes = await _context.MushroomNotes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mushroomNotes == null)
            {
                return NotFound();
            }

            return View(mushroomNotes);
        }

        // POST: MushroomNotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mushroomNotes = await _context.MushroomNotes.FindAsync(id);
            if (mushroomNotes != null)
            {
                _context.MushroomNotes.Remove(mushroomNotes);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MushroomNotesExists(int id)
        {
            return _context.MushroomNotes.Any(e => e.Id == id);
        }
        public IActionResult DownloadPdf(int id)
        {
            var note = _context.MushroomNotes.FirstOrDefault(n => n.Id == id);
            if (note == null) return NotFound();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Content().Column(col =>
                    {
                        col.Item().Text(note.Title).FontSize(20).Bold();
                        col.Item().Text(note.Content).FontSize(12);
                        col.Item().Text($"Utworzono: {note.CreateDate:g}");

                        if (!string.IsNullOrEmpty(note.PhotoUrl))
                        {
                            var path = Path.Combine("wwwroot", note.PhotoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(path))
                                col.Item().Image(path, ImageScaling.FitWidth);
                        }
                    });

                });
            });

            var pdf = document.GeneratePdf();
            return File(pdf, "application/pdf", "notatka.pdf");
        }
        public async Task<IActionResult> Test()
        {
            var note = new MushroomNotes
            {
                Title = "Testowa",
                Content = "Z kontrolera",
                CreateDate = DateTime.Now
            };

            _context.Add(note);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
