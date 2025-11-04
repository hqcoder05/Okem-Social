namespace okem_social.Repositories;

using okem_social.Models;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}