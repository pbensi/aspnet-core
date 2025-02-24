﻿using app.interfaces;
using app.services.Authorizations;
using app.shared;
using app.shared.Dto;
using app.shared.Dto.Account;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Host.HttpContexts;
using Web.Host.Models;

namespace Web.Host.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServicesManager<IAccountService> _accountService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IServicesManager<IAccountService> accountService,
            ILogger<HomeController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<ResponseModel> SwaggerSignIn([FromBody] SecureDataRequest request)
        {
            var response = SecurityUtils.ProcessSecureData<SwaggerSignInModel>(request);

            if (string.IsNullOrEmpty(response.UserName) || string.IsNullOrEmpty(response.Password))
            {
                var validate = new ResponseModel
                {
                    Message = "Username and password are required."
                };

                return validate;
            }

            try
            {
                var (account, message) = await _accountService.Service.SignInAccountAsync(new SignInDto
                {
                    UserName = response.UserName,
                    Password = response.Password
                });

                if (account == null)
                {
                    var responseAccount = new ResponseModel
                    {
                        Message = message
                    };

                    return responseAccount;
                }

                if (!account.IsAdmin)
                {
                    var adminAccount = new ResponseModel
                    {
                        Message = "Account is not an Administrator.",
                    };

                    return adminAccount;
                }

                //var claims = new[]
                // {
                //    new Claim(ClaimTypes.NameIdentifier, account.UserGuid.ToString()),
                //    new Claim(ClaimTypes.Webpage, PageNames.Pages_Swagger)
                //};

                //string token = JwtToken.GenerateJwtToken(claims);

                SessionManager.SetSessionString(HttpContext, SessionNames.Account, account.UserGuid.ToString());
                SessionManager.SetSessionString(HttpContext, SessionNames.Page, PageNames.Pages_Swagger);

                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Success.",
                    RedirectUrl = UrlResources.SwaggerUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during sign-in.");

                throw new Exception(ex.Message);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}