﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM2.Data;
using ASM2.Models;
using ASM2.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace ASM2.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        public BookController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            webHostEnvironment = hostEnvironment;

        }


        // GET: Books
        [Route("/book")]

        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.Include(x => x.Category).ToListAsync());
        }

        // GET: Books/Details/5
        [Route("/book/detail")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var employee = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        [Route("/book/create", Name = "createBook")]

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "Name");
           
            return View();
        }


        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("/book/create/post")]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Price,CategoryId")] Book model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "Name", model.CategoryId);
            return View(model);
        }



        // GET: Employees/Edit/5
        [Route("/book/edit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var employee = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("/book/edit/post")]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [Bind("CategoryId,Title,ReleaseDate,Price,Category,Image,BookCatId")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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
            return View(book);
        }

        // GET: Books/Delete/5
        [Route("/book/delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [Route("/book/delete/post")]
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Employees'  is null.");
            }
            var book = await _context.Books.FindAsync(id);
            if ( book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        public Book getDetailBook(int id)
        {
            var book = _context.Books.Find(id);
            return book;
        }

       
        // Key lưu chuỗi json của Cart
        public const string CARTKEY = "cart";

        // Lấy cart từ Session (danh sách CartItem)

        [Authorize(Roles = "User")]
        public List<Cart> GetCartItems()
        {

            var session = HttpContext.Session;
            string jsoncart = session.GetString(CARTKEY);
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<Cart>>(jsoncart);
            }
            return new List<Cart>();
        }

        // Xóa cart khỏi session
        [Authorize(Roles = "User")]
        public void ClearCart()
        {
            var session = HttpContext.Session;
            session.Remove(CARTKEY);
        }

        // Lưu Cart (Danh sách CartItem) vào session
        [Authorize(Roles = "User")]
        public void SaveCartSession(List<Cart> ls)
        {
            var session = HttpContext.Session;
            string jsoncart = JsonConvert.SerializeObject(ls);
            session.SetString(CARTKEY, jsoncart);
        }



        /// Thêm sản phẩm vào cart
       
        [Authorize(Roles = "User")]
        [Route("/addcart/{bookid:int}", Name = "addcart")]
        public IActionResult AddToCart([FromRoute] int bookid)
        {

            var book = _context.Books
                .Where(p => p.Id == bookid)
                .FirstOrDefault();

            if (book == null)
                return NotFound("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.Books.Id == bookid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.Quantity++;
            }
            else
            {
                //  Thêm mới
                cart.Add(new Cart() { Quantity = 1, Books = book });
            }

            // Lưu cart vào Session
            SaveCartSession(cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction(nameof(Cart));
        }



        /// xóa item trong cart
        [Route("/removecart/{bookid:int}", Name = "removecart")]
        [Authorize(Roles = "User")]
        public IActionResult RemoveCart([FromRoute] int bookid)
        {
            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.Books.Id == bookid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cart.Remove(cartitem);
            }

            SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        /// Cập nhật
        [Authorize(Roles = "User")]
        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int bookid, [FromForm] int quantity)
        {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.Books.Id == bookid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.Quantity = quantity;
            }
            SaveCartSession(cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        // Hiện thị giỏ hàng
        [Route("/cart", Name = "cart")]
        [Authorize(Roles = "User")]
        public IActionResult Cart()
        {
            return View(GetCartItems());
        }

        [Route("/checkout")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CheckOut([FromForm] string email, [FromForm] string address)
        {

            // Xử lý khi đặt hàng
            var cart = GetCartItems();
            ViewData["email"] = email;
            ViewData["address"] = address;
            ViewData["cart"] = cart;

            if (!string.IsNullOrEmpty(email))
            {
                // tạo cấu trúc db lưu lại đơn hàng và xóa cart khỏi session

                Orders orderViewmodel = new Orders
                {
                    Email = email,
                    CartItem = cart.ToString(),
                };
                _context.Add(orderViewmodel);
                await _context.SaveChangesAsync();

                ClearCart();

            }

            return RedirectToAction("Index");
        }



    }
}
