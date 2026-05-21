using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models.Models.Identity;
using TodoListApp.WebApp.Models.Account;
using TodoListApp.WebApp.Services.Email;

namespace TodoListApp.WebApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly EmailService _emailService;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, EmailService emailService)
    {
        this._userManager = userManager;
        this._signInManager = signInManager;
        this._emailService = emailService;
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await this._userManager.GetUserAsync(this.User);
        if (user == null)
        {
            return this.RedirectToAction("Login");
        }

        var model = new ModelProfile
        {
            Username = user.UserName!,
            Email = user.Email!,
        };

        return this.View(model);
    }

    public IActionResult Register()
    {
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(ModelRegister model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var user = new IdentityUser { UserName = model!.Username, Email = model.Email };
        var result = await this._userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return this.RedirectToAction("Login", "Account");
        }

        foreach (var error in result.Errors)
        {
            this.ModelState.AddModelError("", error.Description);
        }

        return this.View(model);
    }

    public IActionResult Login()
    {
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(ModelLogin model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var user = await this._userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var result = await this._signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        return this.View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await this._signInManager.SignOutAsync();
        return this.RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete()
    {
        var user = await this._userManager.GetUserAsync(this.User);
        if (user == null)
        {
            return this.RedirectToAction("Index", "Home");
        }

        var result = await this._userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            await this._signInManager.SignOutAsync();
            return this.RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            this.ModelState.AddModelError("", error.Description);
        }

        var model = new ModelProfile
        {
            Username = user.UserName!,
            Email = user.Email!
        };

        return this.View("Profile", model);
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);

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

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var existingUser = await _userManager.FindByNameAsync(model.Username);

        if (existingUser != null && existingUser.Id != user.Id)
        {
            ModelState.AddModelError("Username", "Username already taken");
            return View(model);
        }

        user.UserName = model.Username;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        return RedirectToAction("Profile");
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new EditPasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(EditPasswordViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword!,
            model.NewPassword!
        );

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Incorrect current password");
            }

            return View(model);
        }
        return RedirectToAction("Profile");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            return View("ForgotPasswordConfirmation");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var link = Url.Action(
            "ResetPassword",
            "Account",
            new { token, email = user.Email },
            Request.Scheme);

        await _emailService.SendEmailAsync(
            user.Email!,
            "Reset Password",
            $"Click here to reset password: <a href='{link}'>Reset Password</a>"
        );
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(
    string token,
    string email)
    {
        return View(new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            model.Token,
            model.NewPassword
        );

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
        return RedirectToAction("Login");
    }
}
