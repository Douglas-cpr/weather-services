using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Precipitation.DataAccess;

public class PrecipDbContext : DbContext {
  public PrecipDbContext() {}
  public PrecipDbContext(DbContextOptions opts) : base(opts) {}
  

  public DbSet<Precipitation> Precipitation { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    SnakeCaseIdentityTableNames(modelBuilder);
  }

  // personal preference, if don't had this method it will map to PascalCase
  private static void SnakeCaseIdentityTableNames(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Precipitation>(b => { b.ToTable("precipitation"); });
  }
}