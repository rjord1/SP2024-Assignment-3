using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SP2024_Assignment_3.Models;

namespace SP2024_Assignment_3.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SP2024_Assignment_3.Models.Actor> Actor { get; set; } = default!;
        public DbSet<SP2024_Assignment_3.Models.Movie> Movie { get; set; } = default!;
        public DbSet<SP2024_Assignment_3.Models.ActorMovie> ActorMovie { get; set; } = default!;
    }
}
