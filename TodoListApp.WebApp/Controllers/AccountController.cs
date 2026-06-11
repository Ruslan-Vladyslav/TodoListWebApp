using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Auth;
using TodoListApp.WebApi.Models.Models.Identity;
using TodoListApp.WebApp.Models.Account;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AccountController(
        IAuthService authService,
        IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return this.View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(ModelRegister model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(new UserRegisterRequest
        {
            Email = model.Email,
            Password = model.Password
        });

        if (result.IsSuccessful)
        {
            return RedirectToAction("Login");
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "Registration failed.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return this.View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(ModelLogin model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(new UserLoginRequest
        {
            Email = model.Email,
            Password = model.Password
        });

        if (result.IsSuccessful)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            var userId = jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
            var email = jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value;
            var name = jwt.Claims.First(x => x.Type == ClaimTypes.Name).Value;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            Response.Cookies.Append("jwt", result.Token);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", result.ErrorMessage ?? "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("jwt");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        await _userService.DeleteAsync(userId);

        return RedirectToAction("Logout");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var model = new ModelProfile
        {
            Username = user.UserName!,
            Email = user.Email!
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var model = new EditProfileViewModel
        {
            Username = user.UserName!,
            Email = user.Email!
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login");
        }

        var currentUser = await _userService.GetByIdAsync(userId);

        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }

        if (!string.Equals(
                currentUser.UserName,
                model.Username,
                StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _userService.UserNameExistsAsync(model.Username);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "Username already taken");

                return View(model);
            }
        }

        await _userService.UpdateUserNameAsync(
            userId,
            model.Username);

        var email = User.FindFirstValue(ClaimTypes.Email) ?? "";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new EditPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(EditPasswordViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login");
        }

        var success = await _authService.ChangePasswordAsync(
            userId,
            model.CurrentPassword!,
            model.NewPassword!);

        if (!success)
        {
            ModelState.AddModelError(
                nameof(model.CurrentPassword),
                "Incorrect current password");

            return View(model);
        }

        return RedirectToAction("Profile");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = await _authService.GeneratePasswordResetTokenAsync(model.Email);

        if (string.IsNullOrEmpty(token))
        {
            return View("ForgotPasswordConfirmation");
        }

        var callbackUrl = Url.Action(
            "ResetPassword",
            "Account",
            new { email = model.Email, token = token },
            protocol: HttpContext.Request.Scheme);

        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string email, string token)
    {
        if (email == null || token == null)
        {
            return BadRequest("Invalid token or email.");
        }

        return View(new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ResetPasswordAsync(
            model.Email,
            model.Token,
            model.NewPassword
        );

        if (result)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        ModelState.AddModelError(string.Empty, "Password reset failed.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}
