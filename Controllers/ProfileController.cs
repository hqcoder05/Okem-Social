using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using okem_social.Models;     // ✅ để dùng okem_social.Models.User
using okem_social.Services;

namespace okem_social.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _env;

    public ProfileController(IUserService userService, IWebHostEnvironment env)
    {
        _userService = userService;
        _env = env;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // -------- Helpers --------
    private string GetAvatarUrl(int userId)
    {
        var p = Path.Combine(_env.WebRootPath, "uploads", "avatars", $"user-{userId}.jpg");
        if (System.IO.File.Exists(p))
        {
            var ver = System.IO.File.GetLastWriteTimeUtc(p).Ticks;
            return $"/uploads/avatars/user-{userId}.jpg?v={ver}";
        }
        return "/img/avatar-default.png";
    }

    // ✅ Sửa: dùng đúng User trong namespace okem_social.Models
    private string GetHandle(User me)
    {
        var core = !string.IsNullOrWhiteSpace(me.Nickname)
            ? me.Nickname!
            : (me.Email?.Split('@')[0] ?? "user");
        return "@" + core;
    }

    // ===== GET: /Profile/Me (read-only) =====
    [HttpGet]
    public async Task<IActionResult> Me()
    {
        var me = await _userService.GetMeAsync(CurrentUserId);

        ViewBag.AvatarUrl       = GetAvatarUrl(CurrentUserId);
        ViewBag.Handle          = GetHandle(me);
        ViewBag.PostsCount      = 0; // TODO: bind thật khi có bảng Posts
        ViewBag.FollowersCount  = await _userService.CountFollowersAsync(CurrentUserId);
        ViewBag.FollowingCount  = await _userService.CountFollowingAsync(CurrentUserId);

        return View(me); // Views/Profile/Me.cshtml
    }

    // ===== GET: /Profile/Edit =====
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var me = await _userService.GetMeAsync(CurrentUserId);
        ViewBag.AvatarUrl = GetAvatarUrl(CurrentUserId);
        ViewBag.Handle    = GetHandle(me);
        return View(me); // Views/Profile/Edit.cshtml
    }

    // ===== POST: /Profile/Edit (chỉ lưu text) =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string fullName, string? nickname, string? bio)
    {
        try
        {
            await _userService.UpdateProfileAsync(CurrentUserId, fullName, bio, nickname);
            TempData["ok"] = "Đã lưu thay đổi.";
            return RedirectToAction(nameof(Me));
        }
        catch (Exception ex)
        {
            TempData["err"] = ex.Message;
            return RedirectToAction(nameof(Edit));
        }
    }

    // ===== POST: /Profile/UploadAvatarJson (AJAX) =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatarJson(IFormFile? avatar)
    {
        if (avatar is null || avatar.Length == 0)
            return BadRequest(new { ok = false, msg = "Chưa chọn ảnh." });

        if (!avatar.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { ok = false, msg = "File không phải ảnh." });

        if (avatar.Length > 2 * 1024 * 1024)
            return BadRequest(new { ok = false, msg = "Ảnh tối đa 2MB." });

        var dir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(dir);

        var outPath = Path.Combine(dir, $"user-{CurrentUserId}.jpg");
        await using (var fs = System.IO.File.Create(outPath))
            await avatar.CopyToAsync(fs);

        var url = GetAvatarUrl(CurrentUserId);
        return Json(new { ok = true, url });
    }

    // ===== POST: /Profile/DeleteAvatarJson (AJAX, tuỳ chọn) =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAvatarJson()
    {
        var path = Path.Combine(_env.WebRootPath, "uploads", "avatars", $"user-{CurrentUserId}.jpg");
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            return Json(new { ok = true });
        }
        return NotFound(new { ok = false, msg = "Không tìm thấy ảnh." });
    }
}
