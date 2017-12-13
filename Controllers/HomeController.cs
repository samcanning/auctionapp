using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Belt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Belt.Controllers
{
    public class HomeController : Controller
    {
        private AuctionContext _context;
 
        public HomeController(AuctionContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegValidator model)
        {
            if(ModelState.IsValid)
            {
                List<User> users = _context.Users.ToList();
                User existing = users.SingleOrDefault(u => u.username == model.username);
                if(existing == null)
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    User newUser = new User
                    {
                        first_name = model.first_name,
                        last_name = model.last_name,
                        username = model.username,
                        wallet = 1000
                    };
                    newUser.password = hasher.HashPassword(newUser, model.password);
                    _context.Add(newUser);
                    _context.SaveChanges();
                    users = _context.Users.ToList();
                    User justCreated = users.Single(u => u.username == newUser.username);
                    HttpContext.Session.SetInt32("id", justCreated.id);
                    HttpContext.Session.SetString("name", justCreated.first_name);
                    return RedirectToAction("Main");
                }
                ModelState.AddModelError("username", "This username is already in use.");              
            }
            return View("Index");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginValidator model)
        {
            if(ModelState.IsValid)
            {
                List<User> users = _context.Users.ToList();
                User attemptedLogin = users.SingleOrDefault(u => u.username == model.log_username);
                if(attemptedLogin == null) 
                {
                    ModelState.AddModelError("log_username", "This username does not exist.");
                }
                else
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    if(0 != hasher.VerifyHashedPassword(attemptedLogin, attemptedLogin.password, model.log_pw))
                    {
                        HttpContext.Session.SetInt32("id", attemptedLogin.id);
                        HttpContext.Session.SetString("name", attemptedLogin.first_name);
                        return RedirectToAction("Main");
                    }
                    else ModelState.AddModelError("log_pw", "Incorrect password.");
                }
            }
            return View("Index");
        }

        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public bool IsNotLogged()
        {
            if(HttpContext.Session.GetInt32("id") != null) return false;
            return true;
        }

        [Route("main")]
        public IActionResult Main()
        {
            if(IsNotLogged()) return RedirectToAction("Index");
            ResolveAllAuctions();
            ViewBag.name = HttpContext.Session.GetString("name");
            ViewBag.id = HttpContext.Session.GetInt32("id");
            List<Auction> auctions = _context.Auctions.ToList();
            auctions = auctions.OrderBy(a => a.end_date).ToList();
            List<User> users = _context.Users.ToList();
            List<DisplayAuction> displayAuctions = new List<DisplayAuction>();
            foreach(Auction a in auctions)
            {
                int days = (a.end_date - DateTime.Now).Days;
                int hours = (a.end_date - DateTime.Now).Hours;
                int minutes = (a.end_date - DateTime.Now).Minutes;
                string time_remaining = $"{days} days, {hours} hours, {minutes} minutes";
                displayAuctions.Add(new DisplayAuction{
                    id = a.id,
                    product = a.product,
                    seller = users.SingleOrDefault(u => u.id == a.creator_id).first_name,
                    seller_id = a.creator_id,
                    top_bid = a.top_bid,
                    time_remaining = time_remaining
                });
            }
            ViewBag.wallet = _context.Users.SingleOrDefault(u => u.id == HttpContext.Session.GetInt32("id")).wallet;
            return View(displayAuctions);
        }

        [Route("create_auction")]
        public IActionResult NewAuction()
        {
            if(IsNotLogged()) return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        [Route("create_action/submit")]
        public IActionResult Create(AuctionValidator model)
        {
            if(IsNotLogged()) return RedirectToAction("Index");
            if(ModelState.IsValid)
            {
                if(model.end_date <= DateTime.Today) ModelState.AddModelError("end_date", "End date must be in the future.");
                if(Double.Parse(model.starting_bid) < 1.00) ModelState.AddModelError("starting_bid", "Starting bid must be at least $1.");
                if(ModelState.IsValid)
                {
                    Auction newAuction = new Auction
                    {
                        product = model.product,
                        description = model.description,
                        end_date = model.end_date,
                        top_bid = decimal.Parse(model.starting_bid),
                        creator_id = (int)HttpContext.Session.GetInt32("id"),
                    };
                    _context.Add(newAuction);
                    _context.SaveChanges();
                    return RedirectToAction("Main");
                }
            }
            return View("NewAuction");
        }

        [Route("delete/{id}")]
        public IActionResult Delete(int id)
        {
            Auction target = _context.Auctions.SingleOrDefault(a => a.id == id);
            if(target == null) return RedirectToAction("Main");
            if(target.creator_id != HttpContext.Session.GetInt32("id")) return RedirectToAction("Main");
            if(target.top_bidder_id != 0)
            {
                User topBidder = _context.Users.SingleOrDefault(u => u.id == target.top_bidder_id);
                topBidder.wallet += target.top_bid;
                _context.Update(topBidder);
            }
            _context.Remove(target);
            _context.SaveChanges();
            return RedirectToAction("Main");
        }
        
        [Route("auction/{id}")]
        public IActionResult DisplayAuction(int id)
        {
            if(IsNotLogged()) return RedirectToAction("Index");
            ResolveAuction(id);
            Auction auction = _context.Auctions.SingleOrDefault(a => a.id == id);
            if(auction == null) return RedirectToAction("Main");
            List<User> users = _context.Users.ToList();
            User seller = users.SingleOrDefault(u => u.id == auction.creator_id);
            string seller_name = $"{seller.first_name} {seller.last_name}";
            int days = (auction.end_date - DateTime.Now).Days;
            int hours = (auction.end_date - DateTime.Now).Hours;
            int minutes = (auction.end_date - DateTime.Now).Minutes;
            string time_remaining = $"{days} days, {hours} hours, {minutes} minutes";
            DisplayAuction displayAuction = new DisplayAuction
            {
                id = auction.id,
                product = auction.product,
                description = auction.description,
                seller = seller_name,
                seller_id = auction.creator_id,
                top_bid = auction.top_bid,
                top_bidder_id = auction.top_bidder_id,
                time_remaining = time_remaining
            };
            User top_bidder = users.SingleOrDefault(u => u.id == auction.top_bidder_id);
            if(top_bidder != null)
            {
                string top_bidder_name = $"{top_bidder.first_name} {top_bidder.last_name}";
                displayAuction.top_bidder = top_bidder_name;
            }
            ViewBag.id = HttpContext.Session.GetInt32("id");
            ViewBag.wallet = _context.Users.SingleOrDefault(u => u.id == HttpContext.Session.GetInt32("id")).wallet;
            return View(displayAuction);
        }

        [HttpPost]
        [Route("auction/{id}/bid")]
        public IActionResult Bid(int id, string bid)
        {
            if(IsNotLogged()) return RedirectToAction("Index");
            ResolveAuction(id);
            Auction auction = _context.Auctions.SingleOrDefault(a => a.id == id);
            if(auction == null) return RedirectToAction("Main");
            decimal validBid = 0;
            if(decimal.TryParse(bid, out validBid) == false) return RedirectToAction("DisplayAuction");
            User user = _context.Users.SingleOrDefault(u => u.id == HttpContext.Session.GetInt32("id"));
            if(validBid > auction.top_bid && validBid <= user.wallet)
            {
                if(auction.top_bidder_id != 0)
                {
                    User previousTopBidder = _context.Users.SingleOrDefault(u => u.id == auction.top_bidder_id);
                    previousTopBidder.wallet += auction.top_bid;
                    _context.Update(previousTopBidder);
                }
                auction.top_bid = validBid;
                user.wallet -= validBid;
                auction.top_bidder_id = user.id;
                _context.Update(auction);
                _context.Update(user);
                _context.SaveChanges();
            }
            return RedirectToAction("DisplayAuction", id);
        }

        public void ResolveAllAuctions()
        {
            List<Auction> auctions = _context.Auctions.ToList();
            foreach(Auction a in auctions)
            {
                if(a.end_date.Date == DateTime.Today)
                {
                    User winner = _context.Users.SingleOrDefault(u => u.id == a.top_bidder_id);
                    User seller = _context.Users.SingleOrDefault(u => u.id == a.creator_id);
                    System.Console.WriteLine($"The auction for {a.product} has ended! The winning bid of {a.top_bid} was made by {winner.first_name} {winner.last_name}.");
                    seller.wallet += a.top_bid;
                    _context.Update(seller);
                    _context.Remove(a);
                }
            }
            _context.SaveChanges();
        }

        public void ResolveAuction(int id)
        {
            Auction auction = _context.Auctions.SingleOrDefault(a => a.id == id);
            if(auction.end_date.Date == DateTime.Today)
            {
                User winner = _context.Users.SingleOrDefault(u => u.id == auction.top_bidder_id);
                User seller = _context.Users.SingleOrDefault(u => u.id == auction.creator_id);
                seller.wallet += auction.top_bid;
                System.Console.WriteLine($"The auction for {auction.product} has ended! The winning bid of {auction.top_bid} was made by {winner.first_name} {winner.last_name}.");
                _context.Remove(auction);
                _context.Update(seller);
                _context.SaveChanges();
            }
        }
    }
}
