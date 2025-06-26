using System;
using System.Collections.Generic;
using Kyrsach.Models;
using Microsoft.EntityFrameworkCore;

namespace Kyrsach.Data;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientService> ClientServices { get; set; }

    public virtual DbSet<DocumentByService> DocumentByServices { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductPhoto> ProductPhotos { get; set; }

    public virtual DbSet<ProductSale> ProductSales { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServicePhoto> ServicePhotos { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=postgres;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScheduleSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DayStart).HasConversion(
                v => v.ToString(),
                v => TimeSpan.Parse(v));
            entity.Property(e => e.DayEnd).HasConversion(
                v => v.ToString(),
                v => TimeSpan.Parse(v));
                
            // Добавляем начальные данные
            entity.HasData(new ScheduleSettings
            {
                Id = 1,
                DayStart = new TimeSpan(9, 0, 0),
                DayEnd = new TimeSpan(21, 0, 0),
                BreakBetweenSlots = 20,
                WorkingDays = 0b0111110 // Пн-Пт
            });
        });
        
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Client_pkey");

            entity.ToTable("Client");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.GenderCode).HasMaxLength(1);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Patronymic).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PhotoPath).HasMaxLength(1000);
            entity.Property(e => e.RegistrationDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Clients)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Client_Gender");

            entity.HasMany(d => d.Tags).WithMany(p => p.Clients)
                .UsingEntity<Dictionary<string, object>>(
                    "TagOfClient",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TagOfClient_Tag"),
                    l => l.HasOne<Client>().WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TagOfClient_Client"),
                    j =>
                    {
                        j.HasKey("ClientId", "TagId").HasName("TagOfClient_pkey");
                        j.ToTable("TagOfClient");
                        j.IndexerProperty<int>("ClientId").HasColumnName("ClientID");
                        j.IndexerProperty<int>("TagId").HasColumnName("TagID");
                    });
        });

        modelBuilder.Entity<ClientService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ClientService_pkey");

            entity.ToTable("ClientService");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.StartTime).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientServices)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientService_Client");

            entity.HasOne(d => d.Service).WithMany(p => p.ClientServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientService_Service");
        });

        modelBuilder.Entity<DocumentByService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DocumentByService_pkey");

            entity.ToTable("DocumentByService");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ClientServiceId).HasColumnName("ClientServiceID");
            entity.Property(e => e.DocumentPath).HasMaxLength(1000);

            entity.HasOne(d => d.ClientService).WithMany(p => p.DocumentByServices)
                .HasForeignKey(d => d.ClientServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentByService_ClientService");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("Gender_pkey");

            entity.ToTable("Gender");

            entity.Property(e => e.Code)
                .HasMaxLength(1)
                .ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(10);
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Manufacturer_pkey");

            entity.ToTable("Manufacturer");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Product_pkey");

            entity.ToTable("Product");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cost).HasPrecision(19, 4);
            entity.Property(e => e.MainImagePath).HasMaxLength(1000);
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Products)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK_Product_Manufacturer");

            entity.HasMany(d => d.AttachedProducts).WithMany(p => p.MainProducts)
                .UsingEntity<Dictionary<string, object>>(
                    "AttachedProduct",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("AttachedProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AttachedProduct_Product1"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("MainProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AttachedProduct_Product"),
                    j =>
                    {
                        j.HasKey("MainProductId", "AttachedProductId").HasName("AttachedProduct_pkey");
                        j.ToTable("AttachedProduct");
                        j.IndexerProperty<int>("MainProductId").HasColumnName("MainProductID");
                        j.IndexerProperty<int>("AttachedProductId").HasColumnName("AttachedProductID");
                    });

            entity.HasMany(d => d.MainProducts).WithMany(p => p.AttachedProducts)
                .UsingEntity<Dictionary<string, object>>(
                    "AttachedProduct",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("MainProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AttachedProduct_Product"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("AttachedProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AttachedProduct_Product1"),
                    j =>
                    {
                        j.HasKey("MainProductId", "AttachedProductId").HasName("AttachedProduct_pkey");
                        j.ToTable("AttachedProduct");
                        j.IndexerProperty<int>("MainProductId").HasColumnName("MainProductID");
                        j.IndexerProperty<int>("AttachedProductId").HasColumnName("AttachedProductID");
                    });
        });

        modelBuilder.Entity<ProductPhoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProductPhoto_pkey");

            entity.ToTable("ProductPhoto");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PhotoPath).HasMaxLength(1000);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPhotos)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductPhoto_Product");
        });

        modelBuilder.Entity<ProductSale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProductSale_pkey");

            entity.ToTable("ProductSale");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ClientServiceId).HasColumnName("ClientServiceID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SaleDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.ClientService).WithMany(p => p.ProductSales)
                .HasForeignKey(d => d.ClientServiceId)
                .HasConstraintName("FK_ProductSale_ClientService");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSales)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductSale_Product");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Service_pkey");

            entity.ToTable("Service");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cost).HasPrecision(19, 4);
            entity.Property(e => e.MainImagePath).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<ServicePhoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ServicePhoto_pkey");

            entity.ToTable("ServicePhoto");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PhotoPath).HasMaxLength(1000);
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

            entity.HasOne(d => d.Service).WithMany(p => p.ServicePhotos)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServicePhoto_Service");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Tag_pkey");

            entity.ToTable("Tag");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Color)
                .HasMaxLength(6)
                .IsFixedLength();
            entity.Property(e => e.Title).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
