using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Options;

namespace BlazorShortUrl.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        // Need for EF scaffolding
        public AppDbContext() { }

        // Need for Ef scaffolding
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseSqlite(Env.GetString("AppDbContext"));
            optionsBuilder.UseSqlServer(Env.GetString("AppDbContext"));
        }

        // Need for Ef scaffolding
        public virtual DbSet<AppUser> ApplicationUser { get; set; }
        public virtual DbSet<AppRole> ApplicationRole { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(p => p.Roles)
                .WithOne()
                .HasForeignKey(p => p.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AppUser>()
                .HasMany(e => e.Claims)
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AppRole>()
                .HasMany(r => r.Claims)
                .WithOne()
                .HasForeignKey(r => r.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // if (Database.ProviderName == "FileBaseContext")
            // {
            //     //https://github.com/dotnet/aspnetcore/issues/21945
            //     builder.Entity<IdentityUserClaim<string>>(entity =>
            //         entity.Property(p => p.Id)
            //               .HasValueGenerator<DummyIdValueGenerator>());
            //     builder.Entity<IdentityRoleClaim<string>>(entity =>
            //         entity.Property(p => p.Id)
            //               .HasValueGenerator<DummyIdValueGenerator>());
            // }
        }
    }
    
    internal class DummyIdValueGenerator : ValueGenerator<int>
    {
        public override bool GeneratesTemporaryValues => false;
        public override int Next(EntityEntry entry) => new Random().Next(1, Int32.MaxValue);
    }
}