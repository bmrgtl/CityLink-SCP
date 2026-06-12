using CityLink_SCP.Common;
using CityLink_SCP.DbModels;
using CityLink_SCP.Extensions;
using CityLink_SCP.Models;
using CityLink_SCP.PageModels;
using CityLink_SCP.Services;
using CityLink_SCP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CityLink_SCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseService _dbService;
        private readonly XmlConfigService _xmlService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(
            ILogger<HomeController> logger,
            DatabaseService dbService,
            XmlConfigService xmlService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _dbService = dbService;
            _xmlService = xmlService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index() => View(GetIndexViewModel());
        public IActionResult News() => View();

        //  Authentication 

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index");
            ViewData["ReturnUrl"] = returnUrl;
            return View(new AdminLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user is ApplicationStaff)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Signin()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signin(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        //  Public pages 

        public IActionResult WhatsOn(string? search, string? sort)
        {
            var vm = GetIndexViewModel();

            var query = _dbService._context.Events.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search) || e.Location.Contains(search));
            query = sort == "date"
                ? query.OrderBy(e => e.Start_Date_Time)
                : sort == "alphabetical"
                    ? query.OrderBy(e => e.Title)
                    : query.OrderByDescending(e => e.Start_Date_Time);

            var filtered = query.ToList().ToCardViewModel();
            if (filtered.Events.Any()) vm.Events = filtered;

            ViewData["Search"] = search;
            ViewData["Sort"] = sort;
            return View(vm);
        }

        [HttpGet]
        public IActionResult Events([ModelBinder(typeof(RestrictedQueryModelBinder))] EventQueryParams query)
        {
            if (query.Size <= 0) query.Size = 12;
            var events = _dbService._context.Events.ApplyQuery(query).ToList();
            SetPublicPagerViewData(query, events.Count == query.Size);
            return View("Events", events);
        }

        [HttpGet]
        public IActionResult SearchPublicEvents([ModelBinder(typeof(RestrictedQueryModelBinder))] EventQueryParams query)
        {
            if (query.Size <= 0) query.Size = 12;
            var events = _dbService._context.Events.ApplyQuery(query).ToList();
            SetPublicPagerViewData(query, events.Count == query.Size);
            return PartialView("_EventCardGrid", events);
        }

        [HttpGet]
        public IActionResult Services([ModelBinder(typeof(RestrictedQueryModelBinder))] ServiceQueryParams query)
        {
            if (query.Size <= 0) query.Size = 12;
            var services = _dbService._context.Services.ApplyQuery(query).ToList();
            SetPublicPagerViewData(query, services.Count == query.Size);
            return View("Services", services);
        }

        [HttpGet]
        public IActionResult SearchPublicServices([ModelBinder(typeof(RestrictedQueryModelBinder))] ServiceQueryParams query)
        {
            if (query.Size <= 0) query.Size = 12;
            var services = _dbService._context.Services.ApplyQuery(query).ToList();
            SetPublicPagerViewData(query, services.Count == query.Size);
            return PartialView("_ServiceCardGrid", services);
        }

        private void SetPublicPagerViewData(QueryParameters query, bool hasMore)
        {
            ViewData["Page"]      = query.Page;
            ViewData["Size"]      = query.Size;
            ViewData["HasMore"]   = hasMore;
            ViewData["SortBy"]    = query.SortBy;
            ViewData["SortOrder"] = query.SortOrder;
        }

        // GET: /Home/ContactUs
        [HttpGet]
        public IActionResult ContactUs() => View(new ContactUs());

        // POST: /Home/ContactUs (logged-in users only — guests use SubmitEnquiry)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactUs(string message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", new { returnUrl = "/Home/ContactUs" });

            var staff = _dbService._context.AppStaff.FirstOrDefault();
            if (staff == null)
            {
                TempData["Error"] = "No staff available to receive your message. Please try again later.";
                return View(new ContactUs());
            }

            _dbService._context.Feedbacks.Add(new Feedback
            {
                Message = message,
                UserId = user.Id,
                StaffId = staff.Id,
                Status = FeedbackStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            _dbService._context.SaveChanges();

            TempData["Success"] = "Thank you! Your message has been received.";
            return RedirectToAction("ContactUs");
        }

        // POST: /Home/SubmitEnquiry  (AJAX — works for guests and logged-in users)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEnquiry(
            string? guestName, string? guestEmail, string? guestPhone,
            string? subject, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, message = "Message cannot be empty." });

            var user   = await _userManager.GetUserAsync(User);
            var staff  = _dbService._context.AppStaff.FirstOrDefault();
            if (staff == null)
                return Json(new { success = false, message = "Unable to submit at this time. Please try again later." });

            var fullMessage = message;
            if (user == null)
            {
                var from = string.Join(", ", new[] { guestName, guestEmail, guestPhone }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                if (!string.IsNullOrWhiteSpace(from))
                    fullMessage = $"[From: {from}]\n\n{message}";
            }
            if (!string.IsNullOrWhiteSpace(subject))
                fullMessage = $"Subject: {subject}\n\n{fullMessage}";

            _dbService._context.Feedbacks.Add(new Feedback
            {
                Message = fullMessage,
                UserId  = user?.Id,
                StaffId = staff.Id,
                Status  = FeedbackStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
            _dbService._context.SaveChanges();

            return Json(new { success = true, message = "Thank you! Your message has been received." });
        }

        // GET: /Home/BookEvent?id=5
        [HttpGet]
        public async Task<IActionResult> BookEvent(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            var ev   = id.HasValue && id > 0
                ? _dbService._context.Events.FirstOrDefault(e => e.Id == id.Value)
                : null;

            return View(new BookEvent
            {
                User = user,
                Event = ev,
                EventRegistration = new EventRegistration { NumberOfAttendees = 1 }
            });
        }

        // POST: /Home/BookEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookEvent(int eventId, int numberOfAttendees)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", new { returnUrl = $"/Home/BookEvent?id={eventId}" });

            var result = _dbService.AddEventRegistration(user.Id, eventId, numberOfAttendees);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new BookEvent
                {
                    User = user,
                    Event = _dbService._context.Events.Find(eventId),
                    EventRegistration = new EventRegistration { NumberOfAttendees = numberOfAttendees }
                });
            }

            TempData["Success"] = "You have successfully registered for the event!";
            return RedirectToAction("Profile");
        }

        // GET: /Home/BookService?id=3
        [HttpGet]
        public async Task<IActionResult> BookService(int? id)
        {
            var user        = await _userManager.GetUserAsync(User);
            var allServices = _dbService._context.Services.ToList();

            ViewData["SelectedServiceId"] = id;
            return View(new BookService
            {
                User             = user,
                AvailableServices = allServices,
                Service          = id.HasValue
                    ? (allServices.FirstOrDefault(s => s.Id == id.Value) ?? new Service())
                    : new Service(),
                ServiceBooking   = new ServiceBooking()
            });
        }

        // POST: /Home/BookService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookService(int serviceId, DateTime startTime, DateTime endTime)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", new { returnUrl = $"/Home/BookService?id={serviceId}" });

            var result = _dbService.AddServiceBooking(user.Id, serviceId, startTime, endTime);
            if (!result.Success)
            {
                var allServices = _dbService._context.Services.ToList();
                TempData["Error"] = result.Message;
                return View(new BookService
                {
                    User              = user,
                    AvailableServices = allServices,
                    Service           = allServices.FirstOrDefault(s => s.Id == serviceId) ?? new Service(),
                    ServiceBooking    = new ServiceBooking { Start_Time = startTime, End_Time = endTime }
                });
            }

            TempData["Success"] = "Your service booking has been confirmed!";
            return RedirectToAction("Profile");
        }

        // GET: /Home/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return View(new ProfileViewModel());

            return View(new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName  = user.LastName,
                Email     = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Address   = user.Address,
                IsStaff   = user is ApplicationStaff,
                JobTitle  = user is ApplicationStaff s ? s.JobTitle : null,

                EventRegistrations = _dbService._context.EventRegistrations
                    .Include(r => r.Event)
                    .Where(r => r.UserId == user.Id)
                    .OrderByDescending(r => r.Event.Start_Date_Time)
                    .ToList(),

                ServiceBookings = _dbService._context.ServiceBookings
                    .Include(b => b.Service)
                    .Where(b => b.UserId == user.Id)
                    .OrderByDescending(b => b.Start_Time)
                    .ToList()
            });
        }

        // POST: /Home/CancelEventRegistration
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEventRegistration(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            var result = _dbService.RemoveEventRegistration(user.Id, eventId);
            if (result.Success) TempData["Success"] = "Registration cancelled.";
            return RedirectToAction("Profile");
        }

        // POST: /Home/CancelServiceBooking
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelServiceBooking(int serviceId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            var booking = await _dbService._context.ServiceBookings.FindAsync(user.Id, serviceId);
            if (booking != null)
            {
                _dbService._context.ServiceBookings.Remove(booking);
                await _dbService._context.SaveChangesAsync();
                TempData["Success"] = "Booking cancelled.";
            }
            return RedirectToAction("Profile");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        #region Helpers
        private IndexViewModel GetIndexViewModel()
        {
            var events        = _dbService._context.Events.ToList().ToCardViewModel();
            var services      = _dbService._context.Services.ToList().ToCardViewModel();
            var faqs          = _xmlService.GetActive<FAQViewModel>();
            var announcements = _xmlService.GetActive<AnnouncementsViewModel>();
            var footer        = _xmlService.GetActive<FooterModel>();
			return new IndexViewModel
            {
                Events        = events.Events.Count > 0 ? events : GetEventsDefault(),
                Services      = services.Services.Count > 0 ? services : GetServicesDefault(),
                FAQs          = faqs?.FAQs?.Count > 0 ? faqs : GetFAQsDefault(),
                Announcements = announcements?.Items?.Count > 0 ? announcements : GetAnnouncementsDefault(),
			};
        }
        private EventsViewModel        GetEventsDefault()        => _xmlService.ToViewModel<EventsViewModel>(System.IO.File.ReadAllText("XML\\EventsDefault.xml"))!;
        private ServicesViewModel      GetServicesDefault()      => _xmlService.ToViewModel<ServicesViewModel>(System.IO.File.ReadAllText("XML\\ServicesDefault.xml"))!;
        private FAQViewModel           GetFAQsDefault()          => _xmlService.ToViewModel<FAQViewModel>(System.IO.File.ReadAllText("XML\\FAQsDefault.xml"))!;
        private AnnouncementsViewModel GetAnnouncementsDefault() => _xmlService.ToViewModel<AnnouncementsViewModel>(System.IO.File.ReadAllText("XML\\AnnouncementsDefault.xml"))!;
		
        #endregion
    }
}
