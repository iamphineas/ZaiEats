using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZaiEats.Data;
using ZaiEats.Models;
using ZaiEats.ViewModels;

namespace ZaiEats.Controllers
{
    public class NewsEventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public NewsEventController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Public: View upcoming news/events/specials
        public async Task<IActionResult> Index()
        {
            var items = await _context.NewsEvents
                .OrderByDescending(e => e.EventDateTime)
                .ToListAsync();

            // Get comment counts for each post
            var commentCounts = await _context.Comments
                .GroupBy(c => c.NewsEventId)
                .Select(g => new { NewsEventId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.NewsEventId, g => g.Count);

            ViewBag.CommentCounts = commentCounts;

            return View(items);
        }


        // Public: View a specific post
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.NewsEvents.FindAsync(id);
            if (item == null) return NotFound();

            var comments = await _context.Comments
                .Where(c => c.NewsEventId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Comments = comments;

            return View(item);
        }

        // Admin: Manage entries
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var items = await _context.NewsEvents
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return View(items);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageComments()
        {
            var comments = await _context.Comments
                .Include(c => c.NewsEvent)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new AdminCommentViewModel
                {
                    Id = c.Id,
                    UserName = c.UserName,
                    Message = c.Message,
                    AdminReply = c.AdminReply,
                    CreatedAt = c.CreatedAt,
                    EventTitle = c.NewsEvent.Title,
                    NewsEventId = c.NewsEventId
                })
                .ToListAsync();

            return View(comments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyToComment(int commentId, string reply)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return NotFound();

            comment.AdminReply = reply;
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageComments");
        }



        // Admin: Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsEvent model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                // Log model validation errors
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        Console.WriteLine($"Validation error on '{key}': {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var imagesPath = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(imagesPath); // ensure folder exists

                    var filePath = Path.Combine(imagesPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    model.ImageUrl = "/images/" + fileName;
                }

                model.CreatedAt = DateTime.UtcNow;
                _context.NewsEvents.Add(model);

                Console.WriteLine("Attempting to save NewsEvent to database...");
                await _context.SaveChangesAsync();
                Console.WriteLine("NewsEvent saved successfully.");

                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during save: " + ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving the news/event.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int id, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return RedirectToAction("Details", new { id });

            var newsEvent = await _context.NewsEvents.FindAsync(id);
            if (newsEvent == null) return NotFound();

            var comment = new Comment
            {
                NewsEventId = id,
                Message = message,
                UserName = User.Identity?.Name ?? "Anonymous"
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var post = await _context.NewsEvents.FindAsync(id);
            if (post == null) return NotFound();

            var userName = User.Identity?.Name ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            // Check if already liked
            var alreadyLiked = await _context.PostLikes
                .AnyAsync(l => l.NewsEventId == id && l.UserName == userName);

            if (!alreadyLiked)
            {
                post.Likes++;
                _context.PostLikes.Add(new PostLike
                {
                    NewsEventId = id,
                    UserName = userName!
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



        // Admin: Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.NewsEvents.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NewsEvent model, IFormFile? ImageFile)
        {
            var item = await _context.NewsEvents.FindAsync(id);
            if (item == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                item.Title = model.Title;
                item.Description = model.Description;
                item.EventDateTime = model.EventDateTime;
                item.Location = model.Location;
                item.DressCode = model.DressCode;
                item.Category = model.Category;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var imagesPath = Path.Combine(_env.WebRootPath, "images");
                    Directory.CreateDirectory(imagesPath);

                    var filePath = Path.Combine(imagesPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    item.ImageUrl = "/images/" + fileName;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during update: " + ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the news/event.");
                return View(model);
            }
        }

        // Admin: Delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.NewsEvents.FindAsync(id);
            if (item == null) return NotFound();

            _context.NewsEvents.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }
    }
}
