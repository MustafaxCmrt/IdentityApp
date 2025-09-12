using System.Security.Claims;
using IdentityApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Controllers;

public class RolesController : Controller
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IdentityContext _context;
    public RolesController(RoleManager<AppRole> roleManager , IdentityContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    // public IActionResult Index()
    // {
    //     return View(_roleManager.Roles);
    // }
    public async Task<IActionResult> Index()
    {
        var claimList = await _context.UserClaims
            .Where(c => c.ClaimType == ClaimTypes.Name
                     || c.ClaimType == ClaimTypes.GivenName
                     || c.ClaimType == ClaimTypes.Surname)
            .Select(c => new { c.UserId, c.ClaimType, c.ClaimValue })
            .AsNoTracking()
            .ToListAsync();

        var displayNameByUserId = claimList
            .GroupBy(c => c.UserId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var name = g.FirstOrDefault(c => c.ClaimType == ClaimTypes.Name)?.ClaimValue;
                    var given = g.FirstOrDefault(c => c.ClaimType == ClaimTypes.GivenName)?.ClaimValue;
                    var surname = g.FirstOrDefault(c => c.ClaimType == ClaimTypes.Surname)?.ClaimValue;
                    var full = !string.IsNullOrWhiteSpace(name) ? name : $"{given} {surname}".Trim();
                    return string.IsNullOrWhiteSpace(full) ? null : full;
                });

        var roleUsersRaw = await (from ur in _context.UserRoles
                                  join u in _context.Users on ur.UserId equals u.Id
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  select new
                                  {
                                      RoleId = r.Id,
                                      RoleName = r.Name,
                                      UserId = u.Id,
                                      UserName = u.UserName,
                                      Email = u.Email
                                  })
                                 .AsNoTracking()
                                 .ToListAsync();

        string GetDisplayName(string userId, string? userName, string? email)
        {
            if (displayNameByUserId.TryGetValue(userId, out var disp) && !string.IsNullOrWhiteSpace(disp))
                return disp!;
            if (!string.IsNullOrWhiteSpace(userName))
                return userName!;
            return email ?? string.Empty;
        }

        var roleUsersByName = roleUsersRaw
            .Where(x => !string.IsNullOrWhiteSpace(x.RoleName))
            .GroupBy(x => x.RoleName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => GetDisplayName(x.UserId, x.UserName, x.Email))
                      .Where(s => !string.IsNullOrWhiteSpace(s))
                      .Distinct()
                      .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var roleUsersById = roleUsersRaw
            .GroupBy(x => x.RoleId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => GetDisplayName(x.UserId, x.UserName, x.Email))
                      .Where(s => !string.IsNullOrWhiteSpace(s))
                      .Distinct()
                      .ToList());

        ViewData["RoleUsersByName"] = roleUsersByName;
        ViewData["RoleUsersById"] = roleUsersById;

        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
        return View(roles);
    }
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppRole model)
    {
        if (ModelState.IsValid)
        {
            var result = await _roleManager.CreateAsync(model);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(model);
    }
}