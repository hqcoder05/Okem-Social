using Microsoft.EntityFrameworkCore;
using okem_social.Data;
using okem_social.Models;

namespace okem_social.Repositories;

public class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email);
}