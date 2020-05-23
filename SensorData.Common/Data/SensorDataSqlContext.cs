using Microsoft.Azure.KeyVault;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;

namespace MySensorData.Common.Data
{
    public partial class SensorDataSqlContext : DbContext
    {
        public SensorDataSqlContext()
        {
            //var keyVaultClient = new KeyVaultClient(AuthenticateVault);
            //var result = await keyVaultClient.GetSecretAsync("https://sensordatavault.vault.azure.net/secrets/ConnectionString/4281c211d3864cb2990f31861ad29e2b");

        }

        public SensorDataSqlContext(DbContextOptions<SensorDataSqlContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Channel> Channel { get; set; }
        public virtual DbSet<DataSource> DataSource { get; set; }
        public virtual DbSet<DataType> DataType { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<SensorData> SensorData { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Channel>(entity =>
            {
                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<DataSource>(entity =>
            {
                entity.HasIndex(e => e.DeviceId)
                    .HasName("UK_DeviceId")
                    .IsUnique();

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.DeviceId)
                    .IsRequired()
                    .HasMaxLength(18);

                entity.Property(e => e.Image).HasColumnType("image");

                entity.HasOne(d => d.DataType)
                    .WithMany(p => p.DataSource)
                    .HasForeignKey(d => d.DataTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DataSource_DataType");
            });

            modelBuilder.Entity<DataType>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("UK_DataType_Name")
                    .IsUnique();

                entity.HasIndex(e => e.Properties)
                    .HasName("UK_DataType_Properties")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Properties)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.DeviceId)
                    .IsRequired()
                    .HasMaxLength(18);

                entity.Property(e => e.LogLevel)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<SensorData>(entity =>
            {
                entity.HasIndex(e => e.DeviceId)
                    .HasName("IX_SensorData_DevideId");

                entity.HasIndex(e => e.TimeStamp);

                entity.Property(e => e.DeviceId)
                    .IsRequired()
                    .HasMaxLength(18);

                entity.Property(e => e.Payload)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Modified).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Password).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        private async Task<string> AuthenticateVault(string authority, string resource, string scope)
        {
            var clientCredential = new ClientCredential("769b1a7c-737d-4bc8-942f-5f2c305e087c", "36NZ3FZiO5biqrqbWugmh?WKOamiJ=/=");
            var authenticateContext = new AuthenticationContext(authority);
            var result = authenticateContext.AcquireTokenAsync(resource, clientCredential);
            return result.Result.AccessToken;
        }

    }
}
