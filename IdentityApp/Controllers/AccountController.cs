using IdentityApp.Data;
using IdentityApp.ViewsModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser>  _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private SignInManager<AppUser> _signInManager;

    public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                await _signInManager.SignOutAsync();
                if(!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError("","confirm your account");
                    return View(model);
                }
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                if (result.Succeeded)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    var lockoutdate = await _userManager.GetLockoutEndDateAsync(user);
                    var timeleft = lockoutdate.Value - DateTime.UtcNow;
                    ModelState.AddModelError("", $"Your account has been locked, please try again in {timeleft.Minutes} minutes.");
                }
                else
                {
                    ModelState.AddModelError("","Your password is incorrect");
                }
            }
        }
        else
        {
            ModelState.AddModelError("","No user found for this email");
        }
        return View(model);
    }
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new AppUser { UserName = "user"+new Random().Next(1,99999), Email = model.Email, FullName = model.FullName };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail", "Account", new { user.Id, token });
                TempData["message"] = "Click on the confirmation email in your email account.";
                return RedirectToAction("Login","Account");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId == null || token == null)
        {
            TempData["message"] = "invalid token information";
            return View();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result =await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["message"] = "Email confirmed";
                return RedirectToAction("Login", "Account");
            }
        }
        TempData["message"] = "user not found";
        return View();

    }
}
