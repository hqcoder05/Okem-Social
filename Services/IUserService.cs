using okem_social.Models;

namespace okem_social.Services;

public interface IUserService
{
    Task<User> GetMeAsync(int currentUserId);

    // mở rộng: cập nhật tên, bio, nickname
    Task UpdateProfileAsync(int currentUserId, string fullName, string? bio, string? nickname);

    Task<User?> GetByIdAsync(int id);
    Task<List<User>> SearchAsync(string keyword, int currentUserId);

    Task<bool> IsFollowingAsync(int currentUserId, int targetUserId);
    Task FollowAsync(int currentUserId, int targetUserId);
    Task UnfollowAsync(int currentUserId, int targetUserId);
    Task<List<User>> FollowersAsync(int userId);
    Task<List<User>> FollowingAsync(int userId);

    Task<int> CountFollowersAsync(int userId);
    Task<int> CountFollowingAsync(int userId);
}