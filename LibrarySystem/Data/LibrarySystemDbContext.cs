using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LibrarySystem.Models;

namespace LibrarySystem.Data;

public partial class LibrarySystemDbContext : DbContext
{
    public LibrarySystemDbContext()
    {
    }

    public LibrarySystemDbContext(DbContextOptions<LibrarySystemDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookCopy> BookCopies { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<Member> Members { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Pk_Authors");

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("ISBN");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_Books_Authors");
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Pk_BookCopies");

            entity.Property(e => e.IsAvailAble).HasDefaultValue(true);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Book).WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_BookCopies_Books");
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_UpdateAvailability"));
            entity.HasKey(e => e.Id).HasName("Pk_Loans");

            entity.HasOne(d => d.BookCopy).WithMany(p => p.Loans)
                .HasForeignKey(d => d.BookCopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Pk_Loans_BookCopies");

            entity.HasOne(d => d.Member).WithMany(p => p.Loans)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Pk_Loans_Members");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Pk_Members");

            entity.HasIndex(e => e.Email, "UQ_Members_Email").IsUnique();

            entity.HasIndex(e => e.MemberNumber, "UQ_Members_MemberNumber").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MemberNumber).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
