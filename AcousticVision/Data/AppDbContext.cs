using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Data;

public class AppDbContext : DbContext
{
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Texture> Textures => Set<Texture>();
    public DbSet<RoomModel> RoomModels => Set<RoomModel>();
    public DbSet<RoomSurface> RoomSurfaces => Set<RoomSurface>();
    public DbSet<SoundSource> SoundSources => Set<SoundSource>();
    public DbSet<SoundReceiver> SoundReceivers => Set<SoundReceiver>();
    public DbSet<TestModel> TestModels => Set<TestModel>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("Materials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.NoiseCancelation).IsRequired();
        });

        modelBuilder.Entity<Texture>(entity =>
        {
            entity.ToTable("Textures");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.NoiseCancelation).IsRequired();
        });

        modelBuilder.Entity<RoomModel>(entity =>
        {
            entity.ToTable("RoomModels");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(250);

            entity.HasIndex(x => x.Name).IsUnique();

            entity.Property(x => x.RoomType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Length).IsRequired();
            entity.Property(x => x.Width).IsRequired();
            entity.Property(x => x.Height).IsRequired();
        });

        modelBuilder.Entity<RoomSurface>(entity =>
        {
            entity.ToTable("RoomSurfaces");
            entity.HasKey(x => new { x.RoomId, x.Position });
            entity.Property(x => x.Position).IsRequired().HasMaxLength(20);

            entity.HasOne(x => x.Room)
                .WithMany(x => x.RoomSurfaces)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Texture)
                .WithMany()
                .HasForeignKey(x => x.TextureId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoundSource>(entity =>
        {
            entity.ToTable("SoundSources");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Volume).IsRequired();
            entity.Property(x => x.Properties).HasMaxLength(500);
        });

        modelBuilder.Entity<SoundReceiver>(entity =>
        {
            entity.ToTable("SoundReceivers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Properties).HasMaxLength(500);
        });

        modelBuilder.Entity<TestModel>(entity =>
        {
            entity.ToTable("TestModels");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SourceLocation)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.ReceiverLocation)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.AnalysisMethod)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(x => x.Room)
                .WithMany(x => x.TestModels)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Source)
                .WithMany()
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.RoomId,
                x.SourceId,
                x.ReceiverId,
                x.SourceLocation,
                x.ReceiverLocation
            }).IsUnique();
        });
    }
}
