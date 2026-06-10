using CityLink_SCP.DbModels;
using CityLink_SCP.Extensions;
using CityLink_SCP.Models;
using CityLink_SCP.PageModels;
using CityLink_SCP.Common;
using CityLink_SCP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Win32;

namespace CityLink_SCP.Controllers
{
	[Authorize(Roles = "Staff")]
	public class AdminController : Controller
	{
		private readonly XmlConfigService _xmlService;
		private readonly DatabaseService _dbService;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ILogger<AdminController> _logger;

		public AdminController(
			XmlConfigService xmlService,
			DatabaseService dbService,
			UserManager<ApplicationUser> userManager,
			ILogger<AdminController> logger)
		{
			_xmlService = xmlService;
			_dbService = dbService;
			_userManager = userManager;
			_logger = logger;
		}

		
		// GET: /Admin  — Dashboard
		public IActionResult Index()
		{
			return View();
		}


        #region XML Config Endpoints

        [HttpGet]
        public IActionResult NewXmlPanel()
        {
            ViewData["AvailableTypes"] = XmlConfigService.GetAvailableTypes();
            return PartialView("_XmlEditorPanel", (XmlConfig?)null);
        }

        [HttpGet]
        public IActionResult GetXmlConfig(int id)
        {
            var config = _dbService._context.XML_Configurations.FirstOrDefault(x => x.Id == id);
            if (config == null) return NotFound();
            ViewData["AvailableTypes"] = XmlConfigService.GetAvailableTypes();
            ViewData["XmlContent"] = _xmlService.GetXmlContentById(id);
            return PartialView("_XmlEditorPanel", config);
        }

        [HttpGet]
		public IActionResult LoadXml(int id)
		{
			var content = _xmlService.GetXmlContentById(id);
			if (content == null) return NotFound();
			return Content(content, "application/xml");
		}

		[HttpGet]
		public IActionResult LoadTemplate(string typeName)
		{
			var template = _xmlService.GenerateTemplate(typeName);
			if (string.IsNullOrEmpty(template)) return NotFound();
			return Content(template, "application/xml");
		}

		[HttpGet]
		public async Task<IActionResult> GetXmlPreview(int id)
		{
			var model = await Task.Run(() => _xmlService.GetXmlViewModel(id));
			if (model == null) return NotFound();
			if (model is not IXmlViewModel xmlVm) return BadRequest("No preview for this type.");
			return PartialView(xmlVm.PartialName, model);
		}

		[HttpPost]
		public async Task<IActionResult> UploadXmlConfig([FromForm] XmlConfigDto xmlConfig)
		{
			if (string.IsNullOrWhiteSpace(xmlConfig.XmlContent)) return BadRequest(new { error = "XML content cannot be empty." });
			if (string.IsNullOrWhiteSpace(xmlConfig.Type)) return BadRequest(new { error = "Config type must be specified." });

			var (valid, error) = _xmlService.Validate(xmlConfig.Type, xmlConfig.XmlContent);
			if (!valid) return BadRequest(new { error = "Invalid XML: " + error });

			try
			{
				var staffId = _userManager.GetUserId(User);
				var staff = _dbService._context.AppStaff.FirstOrDefault(s => s.Id == staffId)
					?? _dbService._context.AppStaff.First();
				await _xmlService.SaveNewVersionAsync(staff, xmlConfig);
				return PartialView("_XmlConfigsTable", XmlConfigsList());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save XML config");
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> ActivateVersion(int recordId)
		{
			var success = await _xmlService.ActivateVersionAsync(recordId);
			if (!success) return BadRequest(new { error = "Config not found." });
			return PartialView("_XmlConfigsTable", XmlConfigsList());
		}

		[HttpGet]
		public IActionResult GetConfigHistory(string typeName)
		{
			var records = _dbService._context.XML_Configurations
				.Where(x => x.Type == typeName)
				.OrderByDescending(x => x.UploadedAt)
				.Select(x => x.ToViewModel())
				.ToList();
			return Json(records);
		}
        [HttpPost]
        public IActionResult DeleteXmlConfig(int id)
        {
            var r = _dbService._context.XML_Configurations.FirstOrDefault(x => x.Id == id);
            if (r == null) return BadRequest(new { error = "Config not found." });
            _dbService._context.XML_Configurations.Remove(r);
            _dbService._context.SaveChanges();
            return PartialView("_XmlConfigsTable", XmlConfigsList());
		}

        private List<XmlConfigDto> XmlConfigsList()
        {
            const int size = 20;
            var items = _dbService._context.XML_Configurations
                .OrderByDescending(x => x.UploadedAt).Take(size)
                .Select(x => x.ToViewModel()).ToList();
            SetPagerViewData(1, size, items.Count == size, "Id", "desc");
            return items;
        }
        #endregion
        #region Users & Staff Endpoints

        [HttpGet]
        public IActionResult GetAllStaff()
        {
            var staff = _dbService._context.AppStaff
                .OrderBy(s => s.LastName)
                .Select(s => new { s.Id, s.FirstName, s.LastName, s.Email, s.PhoneNumber, s.Address, s.JobTitle })
                .ToList();
            return Json(staff);
        }

        [HttpGet]
        public IActionResult NewUserPanel()
        {
            return PartialView("_UserPanel", (ApplicationUser?)null);
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user is ApplicationStaff) return NotFound();
            return PartialView("_UserPanel", user as ApplicationUser);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = "Password is required for new users." });

            var user = new ApplicationUser
            {
                UserName = dto.Email, Email = dto.Email,
                FirstName = dto.First_Name, LastName = dto.Last_Name,
                PhoneNumber = dto.Phone_Number, Address = dto.Address,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });
            return PartialView("_UsersTable", UsersList());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromForm] UserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null || user is ApplicationStaff) return BadRequest(new { error = "User not found." });
            user.FirstName = dto.First_Name; user.LastName = dto.Last_Name;
            user.Email = dto.Email; user.UserName = dto.Email;
            user.PhoneNumber = dto.Phone_Number; user.Address = dto.Address;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, dto.Password);
            }
            return PartialView("_UsersTable", UsersList());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user is ApplicationStaff) return BadRequest(new { error = "User not found." });
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });
            return PartialView("_UsersTable", UsersList());
        }

        private List<ApplicationUser> UsersList()
        {
            const int size = 20;
            var items = _dbService._context.AppUsers
                .Where(u => !(u is ApplicationStaff)).OrderBy(u => u.LastName).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "LastName", "asc");
            return items;
        }

        [HttpGet]
        public IActionResult NewStaffPanel()
        {
            return PartialView("_StaffPanel", (ApplicationStaff?)null);
        }

        [HttpGet]
        public async Task<IActionResult> GetStaff(string id)
        {
            var staff = await _userManager.FindByIdAsync(id) as ApplicationStaff;
            if (staff == null) return NotFound();
            return PartialView("_StaffPanel", staff);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStaff([FromForm] StaffDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = "Password is required for new staff." });

            var staff = new ApplicationStaff
            {
                UserName = dto.Email, Email = dto.Email,
                FirstName = dto.First_Name, LastName = dto.Last_Name,
                PhoneNumber = dto.Phone_Number, Address = dto.Address,
                JobTitle = dto.JobTitle, EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(staff, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });
            await _userManager.AddToRoleAsync(staff, "Staff");
            return PartialView("_StaffTable", StaffList());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStaff([FromForm] StaffDto dto)
        {
            var staff = await _userManager.FindByIdAsync(dto.Id) as ApplicationStaff;
            if (staff == null) return BadRequest(new { error = "Staff member not found." });
            staff.FirstName = dto.First_Name; staff.LastName = dto.Last_Name;
            staff.Email = dto.Email; staff.UserName = dto.Email;
            staff.PhoneNumber = dto.Phone_Number; staff.Address = dto.Address;
            staff.JobTitle = dto.JobTitle;
            var result = await _userManager.UpdateAsync(staff);
            if (!result.Succeeded)
                return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(staff);
                await _userManager.ResetPasswordAsync(staff, token, dto.Password);
            }
            return PartialView("_StaffTable", StaffList());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var staff = await _userManager.FindByIdAsync(id) as ApplicationStaff;
            if (staff == null) return BadRequest(new { error = "Staff member not found." });
            var result = await _userManager.DeleteAsync(staff);
            if (!result.Succeeded)
                return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });
            return PartialView("_StaffTable", StaffList());
        }

        private List<ApplicationStaff> StaffList()
        {
            const int size = 20;
            var items = _dbService._context.AppStaff.OrderBy(s => s.LastName).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "LastName", "asc");
            return items;
        }

        #endregion
        #region Events EndPoints

        [HttpGet]
        public IActionResult NewEventPanel()
        {
            ViewData["Staff"] = _dbService._context.AppStaff.OrderBy(s => s.LastName).ToList();
            return PartialView("_EventPanel", (Event?)null);
        }

        [HttpGet]
        public IActionResult GetEvent(int id)
        {
            var ev = _dbService._context.Events
                .Include(e => e.Staff)
                .FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();
            ViewData["Staff"] = _dbService._context.AppStaff.OrderBy(s => s.LastName).ToList();
            return PartialView("_EventPanel", ev);
        }

        [HttpPost]
        public IActionResult CreateEvent([FromForm] EventDto dto)
        {
            if (!_dbService._context.AppStaff.Any(s => s.Id == dto.StaffId))
                return BadRequest(new { error = "Invalid staff member." });
            var ev = new Event
            {
                Title = dto.Title, Description = dto.Description, Location = dto.Location,
                Cost = dto.Cost, Max_Capcity = dto.Max_Capcity,
                Start_Date_Time = dto.Start_Date_Time, End_Date_Time = dto.End_Date_Time,
                StaffId = dto.StaffId
            };
            _dbService._context.Events.Add(ev);
            _dbService._context.SaveChanges();
            return PartialView("_EventsTable", EventsList());
        }

        [HttpPost]
        public IActionResult UpdateEvent([FromForm] EventDto dto)
        {
            var ev = _dbService._context.Events.FirstOrDefault(e => e.Id == dto.Id);
            if (ev == null) return BadRequest(new { error = "Event not found." });
            ev.Title = dto.Title; ev.Description = dto.Description; ev.Location = dto.Location;
            ev.Cost = dto.Cost; ev.Max_Capcity = dto.Max_Capcity;
            ev.Start_Date_Time = dto.Start_Date_Time; ev.End_Date_Time = dto.End_Date_Time;
            ev.StaffId = dto.StaffId;
            _dbService._context.SaveChanges();
            return PartialView("_EventsTable", EventsList());
        }

        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var ev = _dbService._context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null) return BadRequest(new { error = "Event not found." });
            _dbService._context.Events.Remove(ev);
            _dbService._context.SaveChanges();
            return PartialView("_EventsTable", EventsList());
        }

        private List<Event> EventsList()
        {
            const int size = 20;
            var items = _dbService._context.Events.Include(e => e.Staff)
                .OrderByDescending(e => e.Id).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "Id", "desc");
            return items;
        }

        #endregion
        #region Services

        [HttpGet]
        public IActionResult NewServicePanel()
        {
            ViewData["Staff"] = _dbService._context.AppStaff.OrderBy(s => s.LastName).ToList();
            return PartialView("_ServicePanel", (Service?)null);
        }

        [HttpGet]
        public IActionResult GetService(int id)
        {
            var svc = _dbService._context.Services
                .Include(s => s.Staff)
                .FirstOrDefault(s => s.Id == id);
            if (svc == null) return NotFound();
            ViewData["Staff"] = _dbService._context.AppStaff.OrderBy(s => s.LastName).ToList();
            return PartialView("_ServicePanel", svc);
        }

        [HttpPost]
        public IActionResult CreateService([FromForm] ServiceDto dto)
        {
            if (!_dbService._context.AppStaff.Any(s => s.Id == dto.StaffId))
                return BadRequest(new { error = "Invalid staff member." });
            var svc = new Service
            {
                Title = dto.Title, Description = dto.Description, Location = dto.Location,
                Cost = dto.Cost,
                Available_Start_Time = dto.Available_Start_Time,
                Available_End_Time = dto.Available_End_Time,
                StaffId = dto.StaffId
            };
            _dbService._context.Services.Add(svc);
            _dbService._context.SaveChanges();
            return PartialView("_ServicesTable", ServicesList());
        }

        [HttpPost]
        public IActionResult UpdateService([FromForm] ServiceDto dto)
        {
            var svc = _dbService._context.Services.FirstOrDefault(s => s.Id == dto.Id);
            if (svc == null) return BadRequest(new { error = "Service not found." });
            svc.Title = dto.Title; svc.Description = dto.Description; svc.Location = dto.Location;
            svc.Cost = dto.Cost;
            svc.Available_Start_Time = dto.Available_Start_Time;
            svc.Available_End_Time = dto.Available_End_Time;
            svc.StaffId = dto.StaffId;
            _dbService._context.SaveChanges();
            return PartialView("_ServicesTable", ServicesList());
        }

        [HttpPost]
        public IActionResult DeleteService(int id)
        {
            var svc = _dbService._context.Services.FirstOrDefault(s => s.Id == id);
            if (svc == null) return BadRequest(new { error = "Service not found." });
            _dbService._context.Services.Remove(svc);
            _dbService._context.SaveChanges();
            return PartialView("_ServicesTable", ServicesList());
        }

        private List<Service> ServicesList()
        {
            const int size = 20;
            var items = _dbService._context.Services.Include(s => s.Staff)
                .OrderBy(s => s.Title).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "Title", "asc");
            return items;
        }

        #endregion
        #region Feedback

        [HttpGet]
        public IActionResult GetFeedback(int id)
        {
            var fb = _dbService._context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Staff)
                .FirstOrDefault(f => f.Id == id);
            if (fb == null) return NotFound();
            ViewData["Staff"] = _dbService._context.AppStaff.OrderBy(s => s.LastName).ToList();
            return PartialView("_FeedbackPanel", fb);
        }

        [HttpPost]
        public IActionResult UpdateFeedback([FromForm] FeedbackDto dto)
        {
            var fb = _dbService._context.Feedbacks.FirstOrDefault(f => f.Id == dto.Id);
            if (fb == null) return BadRequest(new { error = "Feedback not found." });
            fb.Status = (FeedbackStatus)dto.Status;
            fb.Resolution_Message = dto.Resolution_Message;
            fb.StaffId = dto.StaffId;
            if (fb.Status == FeedbackStatus.Resolved || fb.Status == FeedbackStatus.Closed)
                fb.ResolvedAt ??= DateTime.UtcNow;
            _dbService._context.SaveChanges();
            return PartialView("_FeedbackTable", FeedbackList());
        }

        [HttpPost]
        public IActionResult DeleteFeedback(int id)
        {
            var fb = _dbService._context.Feedbacks.FirstOrDefault(f => f.Id == id);
            if (fb == null) return BadRequest(new { error = "Feedback not found." });
            _dbService._context.Feedbacks.Remove(fb);
            _dbService._context.SaveChanges();
            return PartialView("_FeedbackTable", FeedbackList());
        }

        private List<Feedback> FeedbackList()
        {
            const int size = 20;
            var items = _dbService._context.Feedbacks.Include(f => f.User).Include(f => f.Staff)
                .OrderByDescending(f => f.Id).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "Id", "desc");
            return items;
        }

        #endregion
        #region Bookings & Registrations

        [HttpPost]
        public IActionResult DeleteEventRegistration(string userId, int eventId)
        {
            var reg = _dbService._context.EventRegistrations
                .FirstOrDefault(r => r.UserId == userId && r.EventId == eventId);
            if (reg == null) return BadRequest(new { error = "Registration not found." });
            _dbService._context.EventRegistrations.Remove(reg);
            _dbService._context.SaveChanges();
            const int size = 20;
            var items = _dbService._context.EventRegistrations
                .Include(r => r.User).Include(r => r.Event)
                .OrderByDescending(r => r.EventId).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "EventId", "desc");
            return PartialView("_EventRegistrationsTable", items);
        }

        [HttpPost]
        public IActionResult DeleteServiceBooking(string userId, int serviceId)
        {
            var booking = _dbService._context.ServiceBookings
                .FirstOrDefault(b => b.UserId == userId && b.ServiceId == serviceId);
            if (booking == null) return BadRequest(new { error = "Booking not found." });
            _dbService._context.ServiceBookings.Remove(booking);
            _dbService._context.SaveChanges();
            const int size = 20;
            var items = _dbService._context.ServiceBookings
                .Include(b => b.User).Include(b => b.Service)
                .OrderByDescending(b => b.Start_Time).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "Start_Time", "desc");
            return PartialView("_ServiceBookingsTable", items);
        }

        #endregion

        //  Search Endpoints — return partial views for AJAX table swap

        #region Search Endpoints

        [HttpGet]
        public IActionResult SearchEvents(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] EventQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var events = _dbService._context.Events.Include(e => e.Staff).ApplyQuery(query).ToList();
            SetPagerViewData(query.Page, query.Size, events.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_EventsTable", events);
        }

        [HttpGet]
        public IActionResult SearchServices(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] ServiceQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var services = _dbService._context.Services.Include(s => s.Staff).ApplyQuery(query).ToList();
            SetPagerViewData(query.Page, query.Size, services.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_ServicesTable", services);
        }

        [HttpGet]
        public IActionResult SearchUsers(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] UserQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var users = _dbService._context.AppUsers
                .Where(u => !(u is ApplicationStaff)).ApplyQuery(query).ToList();
            SetPagerViewData(query.Page, query.Size, users.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_UsersTable", users);
        }

        [HttpGet]
        public IActionResult SearchStaff(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] StaffQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var staff = _dbService._context.AppStaff.ApplyQuery(query).ToList();
            SetPagerViewData(query.Page, query.Size, staff.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_StaffTable", staff);
        }

        [HttpGet]
        public IActionResult SearchFeedback(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] FeedBackQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var feedback = _dbService._context.Feedbacks
                .Include(f => f.User).Include(f => f.Staff).ApplyQuery(query).ToList();
            SetPagerViewData(query.Page, query.Size, feedback.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_FeedbackTable", feedback);
        }

        #endregion

        [HttpGet]
        public IActionResult SearchXmlConfigs(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] XmlConfigQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var configs = _dbService._context.XML_Configurations
                .ApplyQuery(query).ToList()
                .Select(x => x.ToViewModel()).ToList();
            SetPagerViewData(query.Page, query.Size, configs.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_XmlConfigsTable", configs);
        }

        [HttpGet]
        public IActionResult SearchServiceBookings(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] ServiceBookingQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var bookings = _dbService._context.ServiceBookings
                .Include(b => b.User).Include(b => b.Service).ApplyQuery(query).ToList();
            SetPagerViewData(query.Page, query.Size, bookings.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_ServiceBookingsTable", bookings);
        }

        [HttpGet]
        public IActionResult SearchEventRegistrations(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] EventRegistrationQueryParams query)
        {
            if (query.Size <= 1) query.Size = 20;
            var regs = _dbService._context.EventRegistrations
                .Include(r => r.User).Include(r => r.Event).ApplyQuery(query).ToList();
            SetPagerViewData(query.Page, query.Size, regs.Count == query.Size, query.SortBy, query.SortOrder);
            return PartialView("_EventRegistrationsTable", regs);
        }

        private void SetPagerViewData(int page, int size, bool hasMore, string sortBy, string sortOrder)
        {
            ViewData["Page"]      = page;
            ViewData["Size"]      = size;
            ViewData["HasMore"]   = hasMore;
            ViewData["SortBy"]    = sortBy;
            ViewData["SortOrder"] = sortOrder;
        }

        #region Tab Section Endpoints

        [HttpGet]
        public IActionResult GetXmlConfigsSection() =>
            PartialView("_XmlConfigsSection", XmlConfigsList());

        [HttpGet]
        public IActionResult GetUsersSection() =>
            PartialView("_UsersSection", UsersList());

        [HttpGet]
        public IActionResult GetStaffSection() =>
            PartialView("_StaffSection", StaffList());

        [HttpGet]
        public IActionResult GetFeedbackSection() =>
            PartialView("_FeedbackSection", FeedbackList());

        [HttpGet]
        public IActionResult GetServicesSection() =>
            PartialView("_ServicesSection", ServicesList());

        [HttpGet]
        public IActionResult GetEventsSection() =>
            PartialView("_EventsSection", EventsList());

        [HttpGet]
        public IActionResult GetBookingsSection()
        {
            const int size = 20;
            var items = _dbService._context.ServiceBookings
                .Include(b => b.User).Include(b => b.Service)
                .OrderByDescending(b => b.Start_Time).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "Start_Time", "desc");
            return PartialView("_ServiceBookingsSection", items);
        }

        [HttpGet]
        public IActionResult GetRegistrationsSection()
        {
            const int size = 20;
            var items = _dbService._context.EventRegistrations
                .Include(r => r.User).Include(r => r.Event)
                .OrderByDescending(r => r.EventId).Take(size).ToList();
            SetPagerViewData(1, size, items.Count == size, "EventId", "desc");
            return PartialView("_EventRegistrationsSection", items);
        }

        #endregion
    }
}
