using Microsoft.EntityFrameworkCore;

namespace ProcessingScheduler
{
    public class AppDbContext :  DbContext
    {
        public AppDbContext() : base()
        {
        }

        public DbSet<ProcessingRecord> ProcessingRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(@"host=localhost;port=5432;database=test;user id=postgres;password=postgres");
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessingRecord>().HasKey(r => r.ProcessingDate);
            modelBuilder.Entity<ProcessingRecord>().Property(r => r.ProcessingDate).IsRequired();
        }
    }
}