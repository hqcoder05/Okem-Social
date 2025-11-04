using okem_social.Repositories;
using okem_social.Models;

namespace okem_social.Services;

public class AuthService(IUserRepository repo) : IAuthService
{
    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        var user = await repo.GetByEmailAsync(email);
        if (user is null) return null;

        // So khớp mật khẩu (hash bởi BCrypt)
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }
}