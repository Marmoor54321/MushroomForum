using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;

namespace MushroomForum.Controllers
{
    //[Authorize]
    public class ForumThreadsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumThreadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ForumThreads
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumThreads.Include(f => f.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ForumThreads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumThread = await _context.ForumThreads
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.ForumThreadId == id);
            if (forumThread == null)
            {
                return NotFound();
            }

            return View(forumThread);
        }

        // GET: ForumThreads/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ForumThreads/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ForumThreadId,Title,CreatedAt,IdentityUserId")] ForumThread forumThread)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                forumThread.IdentityUserId = userId;

                _context.Add(forumThread);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(forumThread);
        }

        // GET: ForumThreads/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumThread = await _context.ForumThreads.FindAsync(id);
            if (forumThread == null)
            {
                return NotFound();
            }

            if (forumThread == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (forumThread.IdentityUserId != userId)
            {
                return Forbid(); //zabrania edycji jeśli użytkownik chce edytować na innym koncie
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", forumThread.IdentityUserId);
            return View(forumThread);
        }

        // POST: ForumThreads/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ForumThreadId,Title,CreatedAt,IdentityUserId")] ForumThread forumThread)
        {
            if (id != forumThread.ForumThreadId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(forumThread);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ForumThreadExists(forumThread.ForumThreadId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", forumThread.IdentityUserId);
            return View(forumThread);
        }

        // GET: ForumThreads/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumThread = await _context.ForumThreads
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.ForumThreadId == id);
            if (forumThread == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (forumThread.IdentityUserId != userId)
            {
                return Forbid();//zabrania usunięcia jeśli użytkownik chce edytować na innym koncie
            }
            return View(forumThread);
        }

        // POST: ForumThreads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forumThread = await _context.ForumThreads.FindAsync(id);
            if (forumThread != null)
            {
                _context.ForumThreads.Remove(forumThread);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ForumThreadExists(int id)
        {
            return _context.ForumThreads.Any(e => e.ForumThreadId == id);
        }
    }
}
