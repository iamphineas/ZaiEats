using Microsoft.AspNetCore.Mvc;
using ZaiEats.Data;
using ZaiEats.Models;
using ZaiEats.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ZaiEats.Controllers
{
    public class PodcastController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PodcastController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Public View: All Episodes
        public IActionResult Index()
        {
            var episodes = _context.PodcastEpisodes
                .OrderByDescending(e => e.PublishedAt)
                .ToList();

            return View(episodes);
        }

        // Public View: Single Episode
        public IActionResult Details(int id)
        {
            var episode = _context.PodcastEpisodes.Find(id);
            if (episode == null) return NotFound();

            return View(episode);
        }

        // Admin View: Manage Episodes
        [Authorize(Roles = "Admin")]
        public IActionResult Manage()
        {
            var episodes = _context.PodcastEpisodes
                .OrderByDescending(e => e.PublishedAt)
                .ToList();

            return View(episodes);
        }

        // GET: Create Form
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Save New Episode
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PodcastEpisodeViewModel model, IFormFile teaserVideo)
        {
            if (!ModelState.IsValid || teaserVideo == null)
            {
                ModelState.AddModelError("TeaserVideoPath", "Teaser video is required.");
                return View(model);
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "videos");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + Path.GetExtension(teaserVideo.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await teaserVideo.CopyToAsync(fileStream);
            }

            var episode = new PodcastEpisode
            {
                Title = model.Title,
                Description = model.Description,
                TeaserVideoPath = "/videos/" + uniqueFileName,
                FullEpisodeUrl = model.FullEpisodeUrl,
                Tags = model.Tags,
                PublishedAt = DateTime.UtcNow
            };

            _context.PodcastEpisodes.Add(episode);
            _context.SaveChanges();

            return RedirectToAction(nameof(Manage));
        }

        // GET: Edit Form
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var episode = _context.PodcastEpisodes.Find(id);
            if (episode == null) return NotFound();

            var model = new PodcastEpisodeViewModel
            {
                Title = episode.Title,
                Description = episode.Description,
                FullEpisodeUrl = episode.FullEpisodeUrl,
                Tags = episode.Tags
            };

            ViewBag.ExistingTeaserPath = episode.TeaserVideoPath;

            return View(model);
        }

        // POST: Save Edits
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, PodcastEpisodeViewModel model, IFormFile? teaserVideo)
        {
            if (!ModelState.IsValid)
                return View(model);

            var episode = _context.PodcastEpisodes.Find(id);
            if (episode == null) return NotFound();

            if (teaserVideo != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "videos");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(teaserVideo.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await teaserVideo.CopyToAsync(fileStream);
                }

                episode.TeaserVideoPath = "/videos/" + uniqueFileName;
            }

            episode.Title = model.Title;
            episode.Description = model.Description;
            episode.FullEpisodeUrl = model.FullEpisodeUrl;
            episode.Tags = model.Tags;

            _context.Update(episode);
            _context.SaveChanges();

            return RedirectToAction(nameof(Manage));
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var episode = _context.PodcastEpisodes.Find(id);
            if (episode == null) return NotFound();

            _context.PodcastEpisodes.Remove(episode);
            _context.SaveChanges();

            return RedirectToAction(nameof(Manage));
        }
    }
}
