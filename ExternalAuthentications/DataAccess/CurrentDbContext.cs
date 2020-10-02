using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExternalAuthentications.DataAccess
{
    public class CurrentDbContext : IdentityDbContext
    {
        public CurrentDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}