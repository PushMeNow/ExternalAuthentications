using ExternalAuthentications.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExternalAuthentications.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var model = new LoginModel
            {
                ExternalProviders = await _signInManager.GetExternalAuthenticationSchemesAsync()
            };

            return View(model);
        }

        public IActionResult ExternalLogin(string provider)
        {
            string redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "~/")
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            var email = info.Principal.FindFirst(ClaimTypes.Email).Value;
            var userName = info.Principal.FindFirst(ClaimTypes.Name).Value;

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