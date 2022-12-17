using Microsoft.EntityFrameworkCore;

namespace WebapiPlayground.Models
{
    public partial class AlanContext : DbContext
    {
        public AlanContext()
        {
        }

        public AlanContext(DbContextOptions<AlanContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Savedview> Savedviews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=alan;User ID=sa;Password=Nsxrlmed01;trustServerCertificate=true");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Savedview>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("savedviews");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id");
                entity.Property(e => e.Isglobal).HasColumnName("isglobal");
                entity.Property(e => e.Saveobj)
                    .HasMaxLength(50)
                    .HasColumnName("saveobj");
                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp");
                entity.Property(e => e.Viewtype)
                    .HasMaxLength(50)
                    .HasColumnName("viewtype");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
