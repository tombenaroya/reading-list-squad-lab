using Microsoft.EntityFrameworkCore;
using ReadingList.Api.Models;

namespace ReadingList.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Id);
            entity.Property(book => book.Title).IsRequired();
            entity.Property(book => book.Author).IsRequired();
            entity.Property(book => book.Status).HasConversion<string>().IsRequired();
            entity.Property(book => book.Notes).HasMaxLength(Book.MaxNotesLength);
        });
    }
}
