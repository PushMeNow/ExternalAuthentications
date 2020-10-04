using AutoMapper;
using ExternalAuthentications.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExternalAuthentications.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var providers = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var model = new LoginModel
            {
                ExternalProviders = _mapper.Map<IEnumerable<AuthenticationProviderModel>>(providers)
            };

            return View(model);
        }

        public IActionResult ExternalLogin(string providerName)
        {
            string redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(providerName, redirectUrl);

            return new ChallengeResult(providerName, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "~/")
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var userName = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _userManager.FindByNameAsync(email);

                    if (user is null)
                    {
                        user = new IdentityUser
                        {
                            Email = email,
                            UserName = email
                        };

                        var identResult = await _userManager.CreateAsync(user);

                        if (identResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, false);

                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            ModelState.AddModelError("", "User creating error.");
                        }
                    }
                    else
                    {
                        var identResult = await _userManager.AddLoginAsync(user, info);

                        if (identResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, false);

                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Login error.");
                        }
                    }
                }

                return BadRequest(ModelState);
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> ShowUsers()
        {
            var users = await _userManager.Users.Select(q => new UserModel
            {
                Email = q.Email,
                UserName = q.UserName
            }).ToListAsync();

            return View(users);
        }
    }
}