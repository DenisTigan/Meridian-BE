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

        // ── Onboarding ───────────────────────────────────────────────────────
        public DbSet<OnboardingChecklist> OnboardingChecklists { get; set; }
        public DbSet<OnboardingTask> OnboardingTasks { get; set; }

        // ── Buddy System ──────────────────────────────────────────────
        public DbSet<BuddyAssignment> BuddyAssignments { get; set; }

        // ── Announcements ─────────────────────────────────────────────
        public DbSet<Announcement> Announcements { get; set; }

        // ── Quick Links ───────────────────────────────────────────────
        public DbSet<QuickLink> QuickLinks { get; set; }

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

            // ── OnboardingChecklist — relație 1:1 cu Employee ─────────────────
            modelBuilder.Entity<OnboardingChecklist>(entity =>
            {
                entity.HasKey(c => c.Id);

                // Index unic pe EmployeeId — forțează relația 1:1
                entity.HasIndex(c => c.EmployeeId)
                      .IsUnique();

                // FK NOT NULL → Employee, cascade delete
                // (dacă angajatul e șters fizic, checklist-ul dispare și el)
                entity.HasOne(c => c.Employee)
                      .WithMany()
                      .HasForeignKey(c => c.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.OverallProgress)
                      .HasDefaultValue((byte)0);
            });

            // ── OnboardingTask — relație many:1 cu OnboardingChecklist ─────────
            modelBuilder.Entity<OnboardingTask>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(t => t.AutoTriggerType)
                      .HasMaxLength(100);

                // FK NOT NULL → OnboardingChecklists, cascade delete
                entity.HasOne(t => t.Checklist)
                      .WithMany(c => c.Tasks)
                      .HasForeignKey(t => t.ChecklistId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── BuddyAssignment — două FK-uri separate către Employees ──────────────
            // Ambele sunt configurate explicit cu Restrict pentru a evita:
            //   1. Erori de "multiple cascade paths" pe MySQL/SQL Server
            //   2. EF Core genîrerând FK-uri ambiguu denumite prin convenție
            modelBuilder.Entity<BuddyAssignment>(entity =>
            {
                entity.HasKey(a => a.Id);

                // Relația 1: BuddyAssignment → Employee (ca angajat nou)
                entity.HasOne(a => a.NewEmployee)
                      .WithMany()
                      .HasForeignKey(a => a.NewEmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relația 2: BuddyAssignment → Employee (ca buddy)
                entity.HasOne(a => a.Buddy)
                      .WithMany()
                      .HasForeignKey(a => a.BuddyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Announcement ─────────────────────────────────────────────────────
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                // Stochează conținut lung ca TEXT (MySQL)
                entity.Property(a => a.Content)
                      .IsRequired()
                      .HasColumnType("TEXT");

                // Enum stocat ca int (implicit EF Core)
                entity.Property(a => a.Category)
                      .HasConversion<int>();

                // FK NOT NULL → Employees (Author), fără cascade delete
                // (restricție pentru a evita ștergerea accidentală a anunțurilor)
                entity.HasOne(a => a.Author)
                      .WithMany()
                      .HasForeignKey(a => a.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── QuickLink ─────────────────────────────────────────────────────────
            modelBuilder.Entity<QuickLink>(entity =>
            {
                entity.HasKey(q => q.Id);

                entity.Property(q => q.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(q => q.Url)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(q => q.IconName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(q => q.Category)
                      .IsRequired()
                      .HasMaxLength(80);
            });
        }
    }
}