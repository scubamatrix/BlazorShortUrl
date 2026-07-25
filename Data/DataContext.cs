using BlazorShortUrl.Entities;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BlazorShortUrl.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // Need for Ef scaffolding
        public DataContext()
        {
        }

        // Need for EF scaffolding
        public DbSet<ShortUrl> ShortUrls { get; set; }
        // public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlServer(Env.GetString("DataContext"));
            // options.UseSqlServer(Configuration.GetConnectionString("DataContext"));
        }

        // Dispose pattern
        public override void Dispose()
        {
            Log.Debug($"{ContextId} context disposed.");
            base.Dispose();
        }

        // Dispose pattern
        public override ValueTask DisposeAsync()
        {
            Log.Debug($"{ContextId} context disposed async.");
            return base.DisposeAsync();
        }
    }
}