using Microsoft.EntityFrameworkCore;
using ShelterApp.Models;

namespace ShelterApp.Data;

public class ShelterDbContext : DbContext
{
    public ShelterDbContext(DbContextOptions<ShelterDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Enclosure> Enclosures => Set<Enclosure>();
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<VetRecord> VetRecords => Set<VetRecord>();
    public DbSet<AdoptionRequest> AdoptionRequests => Set<AdoptionRequest>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Role>().HasIndex(r => r.Name).IsUnique();
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<Enclosure>().HasIndex(e => e.Number).IsUnique();

        // роль нельзя удалить, пока у неё есть пользователи
        b.Entity<User>().HasOne(u => u.Role).WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

        // при удалении вольера животные остаются, просто без вольера
        b.Entity<Animal>().HasOne(a => a.Enclosure).WithMany(e => e.Animals)
            .HasForeignKey(a => a.EnclosureId).OnDelete(DeleteBehavior.SetNull);

        // пользователя нельзя удалить, пока у него есть записи и заявки
        b.Entity<VetRecord>().HasOne(v => v.User).WithMany(u => u.VetRecords)
            .HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AdoptionRequest>().HasOne(r => r.User).WithMany(u => u.AdoptionRequests)
            .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}