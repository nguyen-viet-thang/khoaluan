using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopPop.Models;
using PopPop.Services.Interfaces;

namespace PopPop.Controllers;

/// <summary>
/// "Tư vấn chọn vợt": a 4-question profile → ranked, explained racket matches.
/// The answers are kept in a cookie so every racket page can show how well it fits this player.
/// </summary>
[AllowAnonymous]
public sealed class AdvisorController(IRacketAdvisorService advisor) : Controller
{
    public const string ProfileCookie = "pp_player";

    [HttpGet]
    public async Task<IActionResult> Index(PlayerProfile profile, CancellationToken cancellationToken)
    {
        // No answers in the URL: continue from the saved profile, if any.
        if (!profile.IsComplete && Request.Query.Count == 0 && ReadSavedProfile(Request) is { } saved)
        {
            profile = saved;
        }

        AdvisorResult? result = null;
        if (profile.IsComplete)
        {
            result = await advisor.RecommendAsync(profile, cancellationToken);
            SaveProfile(profile);
        }

        return View(new AdvisorPageViewModel
        {
            Profile = profile,
            Result = result,
            Brands = await advisor.GetBrandsAsync(cancellationToken)
        });
    }

    /// <summary>Results only, for the instant update while the customer changes an answer.</summary>
    [HttpGet]
    public async Task<IActionResult> Results(PlayerProfile profile, CancellationToken cancellationToken)
    {
        if (!profile.IsComplete)
        {
            return PartialView("_AdvisorResults", null);
        }

        SaveProfile(profile);
        return PartialView("_AdvisorResults", await advisor.RecommendAsync(profile, cancellationToken));
    }

    [HttpGet]
    public IActionResult Reset()
    {
        Response.Cookies.Delete(ProfileCookie);
        return RedirectToAction(nameof(Index), new { restart = 1 });
    }

    public static PlayerProfile? ReadSavedProfile(HttpRequest request) =>
        PlayerProfile.Parse(request.Cookies[ProfileCookie]);

    private void SaveProfile(PlayerProfile profile) =>
        Response.Cookies.Append(ProfileCookie, profile.ToQueryString(), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            MaxAge = TimeSpan.FromDays(180)
        });
}
