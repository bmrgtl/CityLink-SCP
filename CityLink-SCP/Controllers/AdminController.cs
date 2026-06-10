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
			var vm = new AdminIndexViewModel
			{
				XmlConfigs = _dbService._context.XML_Configurations
					.OrderByDescending(x => x.UploadedAt)
					.Select(x => x.ToViewModel())
					.ToList(),

				Events = _dbService._context.Events
					.Include(e => e.Staff)
					.OrderByDescending(x => x.Id)
					.ToList(),

				Services = _dbService._context.Services
					.Include(s => s.Staff)
					.OrderBy(x => x.Title)
					.ToList(),

				Feedbacks = _dbService._context.Feedbacks
					.Include(f => f.User)
					.Include(f => f.Staff)
					.OrderByDescending(x => x.Id)
					.ToList(),

				Users = _dbService._context.AppUsers
					.Where(u => !(u is ApplicationStaff))
					.OrderBy(u => u.LastName)
					.ToList(),

				StaffMembers = _dbService._context.AppStaff
					.OrderBy(s => s.LastName)
					.ToList(),

				ServiceBookings = _dbService._context.ServiceBookings
					.Include(b => b.User)
					.Include(b => b.Service)
					.OrderByDescending(b => b.Start_Time)
					.ToList(),

				EventRegistrations = _dbService._context.EventRegistrations
					.Include(r => r.User)
					.Include(r => r.Event)
					.OrderByDescending(r => r.EventId)
					.ToList(),

				AvailableTypes = XmlConfigService.GetAvailableTypes()
			};

			return View(vm);
		}


        #region XML Config Endpoints
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
			if (string.IsNullOrWhiteSpace(xmlConfig.XmlContent)) return BadRequest("XML content cannot be empty.");
			if (string.IsNullOrWhiteSpace(xmlConfig.Type)) return BadRequest("Config type must be specified.");

			var (valid, error) = _xmlService.Validate(xmlConfig.Type, xmlConfig.XmlContent);
			if (!valid) return BadRequest("Invalid XML: " + error);

			try
			{
				var staffId = _userManager.GetUserId(User);
				var staff = _dbService._context.AppStaff.FirstOrDefault(s => s.Id == staffId)
					?? _dbService._context.AppStaff.First();
				await _xmlService.SaveNewVersionAsync(staff, xmlConfig);
				return Ok(new { message = "Configuration saved successfully." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save XML config");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> ActivateVersion(int recordId)
		{
			var success = await _xmlService.ActivateVersionAsync(recordId);
			if (!success) return NotFound();
			return Ok(new { message = $"Version {recordId} is now active." });
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
            if (r == null) return NotFound();
            _dbService._context.XML_Configurations.Remove(r);
            _dbService._context.SaveChanges();
            return Ok(new { message = $"XML configuration: {r.Label} deleted." });
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
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user is ApplicationStaff) return NotFound();
            return Json(new { user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.Address });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required for new users.");

            var user = new ApplicationUser
            {
                UserName = dto.Email, Email = dto.Email,
                FirstName = dto.First_Name, LastName = dto.Last_Name,
                PhoneNumber = dto.Phone_Number, Address = dto.Address,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            return Ok(new { message = "User created.", id = user.Id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromForm] UserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null || user is ApplicationStaff) return NotFound();
            user.FirstName = dto.First_Name; user.LastName = dto.Last_Name;
            user.Email = dto.Email; user.UserName = dto.Email;
            user.PhoneNumber = dto.Phone_Number; user.Address = dto.Address;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, dto.Password);
            }
            return Ok(new { message = "User updated." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user is ApplicationStaff) return NotFound();
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            return Ok(new { message = "User deleted." });
        }

        [HttpGet]
        public async Task<IActionResult> GetStaff(string id)
        {
            var staff = await _userManager.FindByIdAsync(id) as ApplicationStaff;
            if (staff == null) return NotFound();
            return Json(new { staff.Id, staff.FirstName, staff.LastName, staff.Email, staff.PhoneNumber, staff.Address, staff.JobTitle });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStaff([FromForm] StaffDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required for new staff.");

            var staff = new ApplicationStaff
            {
                UserName = dto.Email, Email = dto.Email,
                FirstName = dto.First_Name, LastName = dto.Last_Name,
                PhoneNumber = dto.Phone_Number, Address = dto.Address,
                JobTitle = dto.JobTitle, EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(staff, dto.Password);
            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            await _userManager.AddToRoleAsync(staff, "Staff");
            return Ok(new { message = "Staff created.", id = staff.Id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStaff([FromForm] StaffDto dto)
        {
            var staff = await _userManager.FindByIdAsync(dto.Id) as ApplicationStaff;
            if (staff == null) return NotFound();
            staff.FirstName = dto.First_Name; staff.LastName = dto.Last_Name;
            staff.Email = dto.Email; staff.UserName = dto.Email;
            staff.PhoneNumber = dto.Phone_Number; staff.Address = dto.Address;
            staff.JobTitle = dto.JobTitle;
            var result = await _userManager.UpdateAsync(staff);
            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(staff);
                await _userManager.ResetPasswordAsync(staff, token, dto.Password);
            }
            return Ok(new { message = "Staff updated." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var staff = await _userManager.FindByIdAsync(id) as ApplicationStaff;
            if (staff == null) return NotFound();
            var result = await _userManager.DeleteAsync(staff);
            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            return Ok(new { message = "Staff deleted." });
        }

        #endregion
        #region Events EndPoints

        [HttpGet]
        public IActionResult GetEvent(int id)
        {
            var ev = _dbService._context.Events
                .Include(e => e.Staff)
                .FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();
            return Json(new
            {
                ev.Id, ev.Title, ev.Description, ev.Location, ev.Cost, ev.Max_Capcity,
                Start_Date_Time = ev.Start_Date_Time.ToString("s"),
                End_Date_Time = ev.End_Date_Time.ToString("s"),
                ev.StaffId
            });
        }

        [HttpPost]
        public IActionResult CreateEvent([FromForm] EventDto dto)
        {
            if (!_dbService._context.AppStaff.Any(s => s.Id == dto.StaffId))
                return BadRequest("Invalid staff member.");
            var ev = new Event
            {
                Title = dto.Title, Description = dto.Description, Location = dto.Location,
                Cost = dto.Cost, Max_Capcity = dto.Max_Capcity,
                Start_Date_Time = dto.Start_Date_Time, End_Date_Time = dto.End_Date_Time,
                StaffId = dto.StaffId
            };
            _dbService._context.Events.Add(ev);
            _dbService._context.SaveChanges();
            return Ok(new { message = "Event created.", id = ev.Id });
        }

        [HttpPost]
        public IActionResult UpdateEvent([FromForm] EventDto dto)
        {
            var ev = _dbService._context.Events.FirstOrDefault(e => e.Id == dto.Id);
            if (ev == null) return NotFound();
            ev.Title = dto.Title; ev.Description = dto.Description; ev.Location = dto.Location;
            ev.Cost = dto.Cost; ev.Max_Capcity = dto.Max_Capcity;
            ev.Start_Date_Time = dto.Start_Date_Time; ev.End_Date_Time = dto.End_Date_Time;
            ev.StaffId = dto.StaffId;
            _dbService._context.SaveChanges();
            return Ok(new { message = "Event updated." });
        }

        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var ev = _dbService._context.Events.FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();
            _dbService._context.Events.Remove(ev);
            _dbService._context.SaveChanges();
            return Ok(new { message = "Event deleted." });
        }

        #endregion
        #region Services

        [HttpGet]
        public IActionResult GetService(int id)
        {
            var svc = _dbService._context.Services
                .Include(s => s.Staff)
                .FirstOrDefault(s => s.Id == id);
            if (svc == null) return NotFound();
            return Json(new
            {
                svc.Id, svc.Title, svc.Description, svc.Location, svc.Cost,
                Available_Start_Time = svc.Available_Start_Time.ToString("HH:mm"),
                Available_End_Time = svc.Available_End_Time.ToString("HH:mm"),
                svc.StaffId
            });
        }

        [HttpPost]
        public IActionResult CreateService([FromForm] ServiceDto dto)
        {
            if (!_dbService._context.AppStaff.Any(s => s.Id == dto.StaffId))
                return BadRequest("Invalid staff member.");
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
            return Ok(new { message = "Service created.", id = svc.Id });
        }

        [HttpPost]
        public IActionResult UpdateService([FromForm] ServiceDto dto)
        {
            var svc = _dbService._context.Services.FirstOrDefault(s => s.Id == dto.Id);
            if (svc == null) return NotFound();
            svc.Title = dto.Title; svc.Description = dto.Description; svc.Location = dto.Location;
            svc.Cost = dto.Cost;
            svc.Available_Start_Time = dto.Available_Start_Time;
            svc.Available_End_Time = dto.Available_End_Time;
            svc.StaffId = dto.StaffId;
            _dbService._context.SaveChanges();
            return Ok(new { message = "Service updated." });
        }

        [HttpPost]
        public IActionResult DeleteService(int id)
        {
            var svc = _dbService._context.Services.FirstOrDefault(s => s.Id == id);
            if (svc == null) return NotFound();
            _dbService._context.Services.Remove(svc);
            _dbService._context.SaveChanges();
            return Ok(new { message = "Service deleted." });
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
            return Json(new
            {
                fb.Id, fb.Message, fb.Resolution_Message,
                Status = (int)fb.Status,
                fb.StaffId,
                From = fb.User != null ? fb.User.FirstName + " " + fb.User.LastName : "(Guest)",
                CreatedAt = fb.CreatedAt.ToString("dd MMM yyyy")
            });
        }

        [HttpPost]
        public IActionResult UpdateFeedback([FromForm] FeedbackDto dto)
        {
            var fb = _dbService._context.Feedbacks.FirstOrDefault(f => f.Id == dto.Id);
            if (fb == null) return NotFound();
            fb.Status = (FeedbackStatus)dto.Status;
            fb.Resolution_Message = dto.Resolution_Message;
            fb.StaffId = dto.StaffId;
            if (fb.Status == FeedbackStatus.Resolved || fb.Status == FeedbackStatus.Closed)
                fb.ResolvedAt ??= DateTime.UtcNow;
            _dbService._context.SaveChanges();
            return Ok(new { message = "Feedback updated." });
        }

        [HttpPost]
        public IActionResult DeleteFeedback(int id)
        {
            var fb = _dbService._context.Feedbacks.FirstOrDefault(f => f.Id == id);
            if (fb == null) return NotFound();
            _dbService._context.Feedbacks.Remove(fb);
            _dbService._context.SaveChanges();
            return Ok(new { message = "Feedback deleted." });
        }

        #endregion
        #region Bookings & Registrations

        [HttpPost]
        public IActionResult DeleteEventRegistration(string userId, int eventId)
        {
            var reg = _dbService._context.EventRegistrations
                .FirstOrDefault(r => r.UserId == userId && r.EventId == eventId);
            if (reg == null) return NotFound();
            _dbService._context.EventRegistrations.Remove(reg);
            _dbService._context.SaveChanges();
            return Ok(new { message = "Registration deleted." });
        }

        [HttpPost]
        public IActionResult DeleteServiceBooking(string userId, int serviceId)
        {
            var booking = _dbService._context.ServiceBookings
                .FirstOrDefault(b => b.UserId == userId && b.ServiceId == serviceId);
            if (booking == null) return NotFound();
            _dbService._context.ServiceBookings.Remove(booking);
            _dbService._context.SaveChanges();
            return Ok(new { message = "Booking deleted." });
        }

        #endregion

        //  Search Endpoints — return partial views for AJAX table swap

        #region Search Endpoints

        [HttpGet]
        public IActionResult SearchEvents(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] EventQueryParams query)
        {
            query.Size = 100;
            var events = _dbService._context.Events
                .Include(e => e.Staff)
                .ApplyQuery(query)
                .ToList();
            return PartialView("_EventsTable", events);
        }

        [HttpGet]
        public IActionResult SearchServices(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] ServiceQueryParams query)
        {
            query.Size = 100;
            var services = _dbService._context.Services
                .Include(s => s.Staff)
                .ApplyQuery(query)
                .ToList();
            return PartialView("_ServicesTable", services);
        }

        [HttpGet]
        public IActionResult SearchUsers(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] UserQueryParams query)
        {
            query.Size = 100;
            var users = _dbService._context.AppUsers
                .Where(u => !(u is ApplicationStaff))
                .ApplyQuery(query)
                .ToList();
            return PartialView("_UsersTable", users);
        }

        [HttpGet]
        public IActionResult SearchStaff(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] StaffQueryParams query)
        {
            query.Size = 100;
            var staff = _dbService._context.AppStaff
                .ApplyQuery(query)
                .ToList();
            return PartialView("_StaffTable", staff);
        }

        [HttpGet]
        public IActionResult SearchFeedback(
            [ModelBinder(typeof(RestrictedQueryModelBinder))] FeedBackQueryParams query)
        {
            query.Size = 100;
            var feedback = _dbService._context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Staff)
                .ApplyQuery(query)
                .ToList();
            return PartialView("_FeedbackTable", feedback);
        }

        #endregion
    }
}
