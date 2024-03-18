using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SP2024_Assignment_3.Data;
using SP2024_Assignment_3.Models;
using VaderSharp2;
using System.Net;
using System.Text.Json;
using System.Web;

namespace SP2024_Assignment_3.Controllers
{
    public class MoviesController : Controller
    {
        public async Task<IActionResult> GetMoviePhoto(int id)
        {
            var movie = await _context.Movie
                .FirstOrDefaultAsync(n => n.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            var imagedata = movie.IMG;

            return File(imagedata, "image/jpg");
        }

        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            MovieDetailsVM mdVM = new MovieDetailsVM();
            mdVM.movie = movie;

            var actors = new List<Actor>();

            actors = await (from a in _context.Actor
                            join am in _context.ActorMovie on a.Id equals am.ActorId
                            where am.MovieId == id
                            select a)
                            .ToListAsync();

            mdVM.actors = actors;

             var queryText = movie.Title;
            var pageContents = await SearchWikipediaAsync(queryText);

            var sentimentList = new List<string>();
            var postList = new List<string>();
            var analyzer = new SentimentIntensityAnalyzer();

            foreach (var pageContent in pageContents)
            {
                // Limit the snippet length to 1500 characters
                var snippet = pageContent.Length > 1500 ? pageContent.Substring(0, 1500) : pageContent;

                // Perform sentiment analysis
                var results = analyzer.PolarityScores(snippet);
                double sentimentScore = results.Compound;

                if (sentimentScore != 0)
                {
                    sentimentList.Add(sentimentScore.ToString() + ", " + CategorizeSentiment(sentimentScore));
                    postList.Add(snippet);
                }
            }

            mdVM.Sentiments = sentimentList;
            mdVM.Posts = postList;

            return View(mdVM);

        }

        public static readonly HttpClient client = new HttpClient();
        public static async Task<List<string>> SearchWikipediaAsync(string queryText)
        {

            string baseUrl = "https://en.wikipedia.org/w/api.php";
            string url = $"{baseUrl}?action=query&list=search&srlimit=100&srsearch={Uri.EscapeDataString(queryText)}&format=json";
            List<string> textToExamine = new List<string>();
            try
            {
                // Ask Wikipedia for a list of pages that relate to the query
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);
                var searchResults = jsonDocument.RootElement.GetProperty("query").GetProperty("search");
                foreach (var item in searchResults.EnumerateArray())
                {
                    var pageId = item.GetProperty("pageid").ToString();
                    // Ask Wikipedia for the text of each page in the query results
                    string pageUrl = $"{baseUrl}?action=query&pageids={pageId}&prop=extracts&explaintext=1&format=json";
                    HttpResponseMessage pageResponse = await client.GetAsync(pageUrl);
                    pageResponse.EnsureSuccessStatusCode();
                    string pageResponseBody = await pageResponse.Content.ReadAsStringAsync();
                    var jsonPageDocument = JsonDocument.Parse(pageResponseBody);
                    var pageContent = jsonPageDocument.RootElement.GetProperty("query").GetProperty("pages").GetProperty(pageId).GetProperty("extract").GetString();
                    textToExamine.Add(pageContent);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return textToExamine;
        }

        public static string CategorizeSentiment(double sentiment)
        {
            if (sentiment >= -1 && sentiment < -0.6)
                return "Extremely Negative";
            else if (sentiment >= -0.6 && sentiment < -0.2)
                return "Very Negative";
            else if (sentiment >= -0.2 && sentiment < 0)
                return "Slightly Negative";
            else if (sentiment >= 0 && sentiment < 0.2)
                return "Slightly Positive";
            else if (sentiment >= 0.2 && sentiment < 0.6)
                return "Very Positive";
            else if (sentiment >= 0.6 && sentiment < 1)
                return "Highly Positive";
            else
                return "Invalid sentiment value";
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDB,Genre,ReleaseYear,IMG")] Movie movie, IFormFile IMG)
        {
            if (ModelState.IsValid)
            {
                if (IMG != null && IMG.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await IMG.CopyToAsync(memoryStream);
                    movie.IMG = memoryStream.ToArray();
                }
                else
                {
                    movie.IMG = new byte[0];
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDB,Genre,ReleaseYear,IMG")] Movie movie, IFormFile IMG)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(movie.IMG));

            Movie existingMovie = _context.Movie.AsNoTracking().FirstOrDefault(m => m.Id == id);

            if (IMG != null && IMG.Length > 0)
            {
                var memoryStream = new MemoryStream();
                await IMG.CopyToAsync(memoryStream);
                movie.IMG = memoryStream.ToArray();
            }
            else if (existingMovie != null)
            {
                movie.IMG = existingMovie.IMG;
            }
            else
            {
                movie.IMG = new byte[0];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
