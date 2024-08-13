using Microsoft.EntityFrameworkCore;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Infrastructure.Data
{
    public class DataEntities : DbContext
    {
        public DataEntities(DbContextOptions<DataEntities> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<ValidationLog> ValidationLogs { get; set; }
        public DbSet<BillsPayment> BillsPayments { get; set; }

        public DbSet<FundTransfer> FundTransfers { get; set; }
        public DbSet<CustomerDebit> CustomerDebits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasConversion(
                    v => v.ToString(),
                    v => v == null ? null : v
                );

            base.OnModelCreating(modelBuilder);
        }
    }

}
