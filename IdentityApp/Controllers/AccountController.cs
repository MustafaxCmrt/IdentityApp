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

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string Email)
    {
        if (!string.IsNullOrEmpty(Email))
        {
            TempData["message"] = "Please check your email";
            return View();
        }
           
        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            TempData["message"] = "Email not found";
            return View();
        }
            
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = Url.Action("ResetPassword", "Account", new { user.Id, token });
        
        // await _smtpEmailSender.SendEmailAsync((Email,"password reset",$"Click on the link to reset your password <a href=enter your own localhost address'{url}'>{url}</a>"));
        // burada email göndermek için kendi localhost adresinizi girin(daha önce hosting adresiniz varsa) 
        TempData["message"] = "Click on the reset email in your email account.";
        return View();
    }

    public IActionResult ResetPassword(string Id, string token)
    {
        if (Id == null || token == null)
        {
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordModel{Token = token};
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["message"] = "Email not found";
                return RedirectToAction("Login");
            }
                
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["message"] = "Password reset completed";
                return RedirectToAction("Login");
            }
                
            foreach (var error in result.Errors)
                {
                  ModelState.AddModelError(string.Empty, error.Description);
                }
        }
        return View(model);
    }
}
