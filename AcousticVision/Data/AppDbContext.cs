using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AcousticVision.Data;

public class AppDbContext : DbContext
{
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Texture> Textures => Set<Texture>();
    public DbSet<Wall> Walls => Set<Wall>();
    public DbSet<RoomModel> RoomModels => Set<RoomModel>();
    public DbSet<RoomWall> RoomWalls => Set<RoomWall>();
    public DbSet<SoundSource> SoundSources => Set<SoundSource>();
    public DbSet<SoundReceiver> SoundReceivers => Set<SoundReceiver>();
    public DbSet<TestModel> TestModels => Set<TestModel>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Materials
        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("Materials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.NoiseCancelation).IsRequired();
        });

        // Textures
        modelBuilder.Entity<Texture>(entity =>
        {
            entity.ToTable("Textures");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.NoiseCancelation).IsRequired();
        });

        // Walls
        modelBuilder.Entity<Wall>(entity =>
        {
            entity.ToTable("Walls");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Width).IsRequired();
            entity.Property(x => x.Height).IsRequired();

            entity.HasOne(x => x.Material)
                .WithMany(x => x.Walls)
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Texture)
                .WithMany(x => x.Walls)
                .HasForeignKey(x => x.TextureId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RoomModels
        modelBuilder.Entity<RoomModel>(entity =>
        {
            entity.ToTable("RoomModels");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();

            entity.Property(x => x.Length).IsRequired();
            entity.Property(x => x.Width).IsRequired();
            entity.Property(x => x.Height).IsRequired();
        });

        // RoomWalls
        modelBuilder.Entity<RoomWall>(entity =>
        {
            entity.ToTable("RoomWalls");
            entity.HasKey(x => new { x.RoomId, x.WallId });

            entity.Property(x => x.Position)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(x => new { x.RoomId, x.Position }).IsUnique();

            entity.HasOne(x => x.Room)
                .WithMany(x => x.RoomWalls)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Wall)
                .WithMany(x => x.RoomWalls)
                .HasForeignKey(x => x.WallId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SoundSources
        modelBuilder.Entity<SoundSource>(entity =>
        {
            entity.ToTable("SoundSources");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();

            entity.Property(x => x.Location).IsRequired().HasMaxLength(250);
            entity.Property(x => x.Volume).IsRequired();
            entity.Property(x => x.Properties).HasMaxLength(500);
        });

        // SoundReceivers
        modelBuilder.Entity<SoundReceiver>(entity =>
        {
            entity.ToTable("SoundReceivers");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired().HasMaxLength(250);
            entity.HasIndex(x => x.Name).IsUnique();

            entity.Property(x => x.Location).IsRequired().HasMaxLength(250);
            entity.Property(x => x.Properties).HasMaxLength(500);
        });

        // TestModels
        modelBuilder.Entity<TestModel>(entity =>
        {
            entity.ToTable("TestModels");
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.RoomId, x.SourceId, x.ReceiverId }).IsUnique();

            entity.HasOne(x => x.Room)
                .WithMany(x => x.TestModels)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Source)
                .WithMany(x => x.TestModels)
                .HasForeignKey(x => x.SourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Receiver)
                .WithMany(x => x.TestModels)
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}