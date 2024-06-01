using api.service.abstractions;
using api.shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using Web.Host.Models;

namespace Web.Host.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IServicesManager _servicesManager;
        private readonly HealthCheckService _healthCheckService;

        public LoginController(ILogger<LoginController> logger,
          IServicesManager servicesManager,
          HealthCheckService healthCheckService)
        {
            _logger = logger;
            _servicesManager = servicesManager;
            _healthCheckService = healthCheckService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _servicesManager.UserService.SignInUserAsync(request.AccountName, request.Password);
            string messageFailed = "Invalid username or password. Please try again.";

            if (result.user != null)
            {
                if (result.user.IsAdmin)
                {
                    HttpContext.Session.SetString("token", result.encryptedId);

                    return Redirect("/swagger");
                }

                messageFailed = "Account is not an Administrator.";
            }

            TempData["ErrorMessage"] = messageFailed;
            return RedirectToAction("Index", "Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
