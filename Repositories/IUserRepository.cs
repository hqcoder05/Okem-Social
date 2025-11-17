using okem_social.Models;

namespace okem_social.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task UpdateAsync(User user);

    // 🔹 Search mở rộng: fullname/email/nickname (case-insensitive)
    Task<List<User>> SearchAsync(string keyword, int excludeUserId, int take = 50);

    // Follow
    Task<bool> IsFollowingAsync(int followerId, int followeeId);
    Task AddFollowAsync(int followerId, int followeeId);
    Task RemoveFollowAsync(int followerId, int followeeId);
    Task<List<User>> GetFollowersAsync(int userId);
    Task<List<User>> GetFollowingAsync(int userId);
    Task<int> CountFollowersAsync(int userId);
    Task<int> CountFollowingAsync(int userId);

    // 🔹 Nickname
    Task<User?> FindByNicknameAsync(string nickname);
    Task<bool> NicknameExistsAsync(string nickname, int exceptUserId = 0);
}