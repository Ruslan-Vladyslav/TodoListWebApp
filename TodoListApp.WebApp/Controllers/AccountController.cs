using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models.Models.Identity;

namespace TodoListApp.WebApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        this._userManager = userManager;
        this._signInManager = signInManager;
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

}
