using MeridianEmployeeHub.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Context
{
    // DbContext este "inima" Entity Framework. El gestionează conexiunea și maparea claselor la tabele.
    public class ApplicationDbContext : DbContext
    {
        // Constructorul care primește opțiunile de conectare din afară (din API)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ── DbSets — fiecare reprezintă un tabel în baza de date ─────────────
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Role> Roles { get; set; }

        // ── Auto-set CreatedAt / UpdatedAt la fiecare salvare ────────────────
        // CreatedBy / UpdatedBy vor fi populate în sesiunea de Auth (IHttpContextAccessor)
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }

            // Auto-set CreatedAt pentru Department (nu moștenește BaseEntity)
            foreach (var entry in ChangeTracker.Entries<Department>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Query filter global pentru soft-delete ───────────────────────
            // Angajații inactivi sunt excluși automat din ORICE query
            modelBuilder.Entity<Employee>().HasQueryFilter(e => e.IsActive);

            // ── Role — configurare lookup table ──────────────────────────────
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // ── Department — configurare ──────────────────────────────────────
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                // FK nullable → Employee (HeadEmployee) — fără cascade delete
                entity.HasOne(d => d.HeadEmployee)
                      .WithMany()
                      .HasForeignKey(d => d.HeadEmployeeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Team — configurare ────────────────────────────────────────────
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                // FK NOT NULL → Department
                entity.HasOne(t => t.Department)
                      .WithMany(d => d.Teams)
                      .HasForeignKey(t => t.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK nullable → Employee (TeamLead)
                entity.HasOne(t => t.TeamLead)
                      .WithMany()
                      .HasForeignKey(t => t.TeamLeadId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Employee — configurare FK-uri și indecși ──────────────────────
            modelBuilder.Entity<Employee>(entity =>
            {
                // Index unic pe Email
                entity.HasIndex(e => e.Email)
                      .IsUnique();

                // FK NOT NULL → Department
                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK nullable → Team
                entity.HasOne(e => e.Team)
                      .WithMany(t => t.Members)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);

                // FK NOT NULL → Role
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Employees)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Self-referencing FK nullable → Manager (Employee)
                entity.HasOne(e => e.Manager)
                      .WithMany(m => m.Subordinates)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // CreatedBy / UpdatedBy — FK nullable spre Employees, fără relație navigabilă
                // (evităm cicluri de FK multiple pe același tabel)
                entity.HasOne<Employee>()
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne<Employee>()
                      .WithMany()
                      .HasForeignKey(e => e.UpdatedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}