using Elfie.Serialization;
using janaez.webapi.Models;
using Microsoft.EntityFrameworkCore;

namespace janaez.webapi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Funeral> Funerals { get; set; }
        public DbSet<Prayer> Prayers { get; set; }
        public DbSet<Sex> Sexes { get; set; }
        public DbSet<PurialPlace> PurialPlaces { get; set; }

        public DbSet<SqlQueries> SqlQueries { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Funeral>().HasKey(f => f.Id);
            modelBuilder.Entity<Funeral>().ToTable("funerals");

            modelBuilder.Entity<Funeral>()
                .HasOne(f => f.Prayer)
                .WithMany()
                .HasForeignKey(f => f.PrayerId);

            modelBuilder.Entity<Funeral>()
                .HasOne(f => f.Sex)
                .WithMany()
                .HasForeignKey(f => f.SexId);

            modelBuilder.Entity<Funeral>()
                .HasOne(f => f.PurialPlace)
                .WithMany()
                .HasForeignKey(f => f.PurialPlaceId);

            modelBuilder.Entity<Prayer>().ToTable("prayers");
            modelBuilder.Entity<Prayer>().HasKey(p => p.id);

            modelBuilder.Entity<Sex>().HasKey(s => s.id);
            modelBuilder.Entity<Sex>().ToTable("sexes");

            modelBuilder.Entity<PurialPlace>().HasKey(p => p.id);
            modelBuilder.Entity<PurialPlace>().ToTable("purialplaces");

            modelBuilder.Entity<SqlQueries>().HasKey(p => p.Key);

        }
    }
}
