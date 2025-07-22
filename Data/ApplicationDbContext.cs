using GoogleLogin.Models;
using Microsoft.EntityFrameworkCore;

namespace GoogleLogin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<UserInfo> UserInfos { get; set; }
    }
}
