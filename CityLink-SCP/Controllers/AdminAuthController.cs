using CityLink_SCP.DbModels;
using CityLink_SCP.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CityLink_SCP.Controllers
{
    /// <summary>
    /// Handles staff authentication only.
    /// The AdminController (index, CRUD) is separately guarded with [Authorize(Roles="Staff")].
    /// </summary>
    [Route("Admin")]
    public class AdminAuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminAuthController> _logger;

        public AdminAuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminAuthController> logger)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
            _logger        = logger;
        }

        // GET: /Admin/Login  — uses the same shared login view as the public site
        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Staff"))
                return RedirectToAction("Index", "Admin");

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginTitle"] = "Staff Portal";
            return View("~/Views/Home/Login.cshtml", new AdminLoginViewModel());
        }

        // POST: /Admin/Login
        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Check the account exists AND is a staff member
            if (user == null || user is not ApplicationStaff)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Staff {Email} logged in.", model.Email);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Staff account {Email} locked out.", model.Email);
                ModelState.AddModelError(string.Empty, "Account locked. Try again in 5 minutes.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        // POST: /Admin/Logout
        [HttpPost("Logout")]
        [Authorize(Roles = "Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Staff signed out.");
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Admin");
        }
    }
}
