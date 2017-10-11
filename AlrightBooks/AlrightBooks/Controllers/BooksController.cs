﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlrightBooks.Data;
using AlrightBooks.Models;
using AlrightBooks.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;

namespace AlrightBooks.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)

        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            Books testBook = new Books();
            testBook.User = await _userManager.GetUserAsync(User);
            return View(await _context.Books.ToListAsync());
        }


       // [HttpGet("[action]/{genre}")]
        public async Task<IActionResult> Genre(string genre)
        {
            ICollection<Books> ReturnBooks = new List<Books>();
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("https://www.googleapis.com");
                    var response = await client.GetAsync($"/books/v1/volumes?maxResults=40&q=subject:{genre}");
                    response.EnsureSuccessStatusCode();
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var rawBooks = TheBooks.FromJson(stringResult);
                    IEnumerable<Item> RawBooks = from o in rawBooks.Items
                                                 where o.VolumeInfo.Description != null
                                                 select o;
                    foreach (var o in RawBooks)
                    {
                        decimal? temp = 0.00M;
                        if (o.VolumeInfo.AverageRating == null)
                        {
                            temp = 0.00M;
                        }
                        else
                        {
                            temp = o.VolumeInfo.AverageRating;
                        }
                        string tempISBN = "N/A"; 
                        if (o.VolumeInfo.IndustryIdentifiers != null)
                        {
                            tempISBN = o.VolumeInfo.IndustryIdentifiers[0].Identifier;
                        }
                        Books Abook = new Books
                        {
                            Title = o.VolumeInfo.Title,
                            Author = o.VolumeInfo.Authors[0],
                            AvgRating = temp,
                            Description = o.VolumeInfo.Description,
                            ImgURL = o.VolumeInfo.ImageLinks.Thumbnail,
                            ISBN = tempISBN
                        };
                        ReturnBooks.Add(Abook);
                    }
                    return View(ReturnBooks);
                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Error getting requested books from Google Books: {httpRequestException.Message}");
                }
            }
        }


        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var books = await _context.Books
                .SingleOrDefaultAsync(m => m.BookID == id);
            if (books == null)
            {
                return NotFound();
            }

            return View(books);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookID,Author,AvgRating,Title,ImgURL,ISBN")] Books books)
        {
            if (ModelState.IsValid)
            {
                _context.Add(books);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(books);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var books = await _context.Books.SingleOrDefaultAsync(m => m.BookID == id);
            if (books == null)
            {
                return NotFound();
            }
            return View(books);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookID,Author,AvgRating,Title,ImgURL,ISBN")] Books books)
        {
            if (id != books.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(books);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BooksExists(books.BookID))
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
            return View(books);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var books = await _context.Books
                .SingleOrDefaultAsync(m => m.BookID == id);
            if (books == null)
            {
                return NotFound();
            }

            return View(books);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var books = await _context.Books.SingleOrDefaultAsync(m => m.BookID == id);
            _context.Books.Remove(books);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BooksExists(int id)
        {
            return _context.Books.Any(e => e.BookID == id);
        }
    }
}