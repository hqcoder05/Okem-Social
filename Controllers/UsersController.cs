using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using okem_social.Services;

namespace okem_social.Controllers;

[Authorize]
public class UsersController(IUserService userService) : Controller
{
    // Dùng ViewerId an toàn cho action AllowAnonymous
    private int? ViewerId =>
        User.Identity?.IsAuthenticated == true
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            : (int?)null;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var u = await userService.GetByIdAsync(id);
        if (u == null) return NotFound();

        // Thống kê
        ViewBag.FollowersCount = await userService.CountFollowersAsync(id);
        ViewBag.FollowingCount = await userService.CountFollowingAsync(id);
        ViewBag.PostsCount = 0; // chưa làm Post

        // Ngữ cảnh
        ViewBag.IsOwner = ViewerId.HasValue && ViewerId.Value == id;
        ViewBag.IsFollowing = (!ViewBag.IsOwner && ViewerId.HasValue)
            ? await userService.IsFollowingAsync(ViewerId.Value, id)
            : false;

        // UI helpers (không đổi DB)
        ViewBag.Handle = "@" + (u.Email?.Split('@')[0].ToLower() ?? "user");
        ViewBag.AvatarUrl = "/img/avatar-default.png";

        return View(u); // Model là User
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? keyword)
    {
        // Class có [Authorize] nên chắc chắn đã đăng nhập
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var results = await userService.SearchAsync(keyword ?? "", currentUserId);
        ViewBag.Keyword = keyword ?? "";
        return View(results);
    }

    [HttpGet]
    public async Task<IActionResult> Followers(int id)
    {
        var target = await userService.GetByIdAsync(id);
        if (target == null) return NotFound();

        ViewBag.Target = target;
        var followers = await userService.FollowersAsync(id);

        // Ép đường dẫn tuyệt đối để tránh lỗi View Not Found
        return View("~/Views/Users/Followers.cshtml", followers);
    }

    [HttpGet]
    public async Task<IActionResult> Following(int id)
    {
        var target = await userService.GetByIdAsync(id);
        if (target == null) return NotFound();

        ViewBag.Target = target;
        var following = await userService.FollowingAsync(id);

        return View("~/Views/Users/Following.cshtml", following);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Follow(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await userService.FollowAsync(currentUserId, id);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unfollow(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await userService.UnfollowAsync(currentUserId, id);
        return RedirectToAction(nameof(Details), new { id });
    }
}
