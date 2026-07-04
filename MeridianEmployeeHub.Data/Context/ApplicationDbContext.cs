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

        // ── Desk Booking ───────────────────────────────────────────────
        public DbSet<Office> Offices { get; set; }
        public DbSet<Desk> Desks { get; set; }
        public DbSet<DeskBooking> DeskBookings { get; set; }

        // ── Company Calendar ───────────────────────────────────────────────
        public DbSet<CalendarEvent> CalendarEvents { get; set; }

        // ── HR Tickets ─────────────────────────────────────────────────────────
        public DbSet<HRTicket> HRTickets { get; set; }

        // ── Leave Requests & Balance ───────────────────────────────────────────
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }

        // ── Notifications ──────────────────────────────────────────────────────
        public DbSet<Notification> Notifications { get; set; }

        // ── Training Center ────────────────────────────────────────────────────
        public DbSet<TrainingCourse> TrainingCourses { get; set; }
        public DbSet<TrainingModule> TrainingModules { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }

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

            // Auto-set CreatedAt pentru Office (nu moștenește BaseEntity)
            foreach (var entry in ChangeTracker.Entries<Office>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }
            }

            // Auto-set CreatedAt pentru DeskBooking (nu moștenește BaseEntity)
            foreach (var entry in ChangeTracker.Entries<DeskBooking>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }
            }

            // Auto-set CreatedAt pentru CalendarEvent (nu moștenește BaseEntity)
            foreach (var entry in ChangeTracker.Entries<CalendarEvent>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }
            }

            // Auto-set CreatedAt/UpdatedAt pentru HRTicket (nu moștenește BaseEntity)
            foreach (var entry in ChangeTracker.Entries<HRTicket>())
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

            // Auto-set CreatedAt/UpdatedAt pentru LeaveRequest (nu moștenește BaseEntity)
            foreach (var entry in ChangeTracker.Entries<LeaveRequest>())
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

            // Auto-set CreatedAt pentru Notification (nu moștenește BaseEntity)
            foreach (var entry in ChangeTracker.Entries<Notification>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }
            }

            // Auto-set CreatedAt/UpdatedAt pentru TrainingCourse
            foreach (var entry in ChangeTracker.Entries<TrainingCourse>())
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

            // ── Office ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Office>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(o => o.Address)
                      .IsRequired()
                      .HasMaxLength(300);
            });

            // ── Desk ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Desk>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.DeskCode)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(d => d.Zone)
                      .IsRequired()
                      .HasMaxLength(50);

                // Coordonate hartă interactivă (Milestone 5) — precision(6,2)
                entity.Property(d => d.PositionX)
                      .HasPrecision(6, 2);

                entity.Property(d => d.PositionY)
                      .HasPrecision(6, 2);

                // FK NOT NULL → Offices, Restrict (desk-urile nu dispar dacă office-ul ar fi șters)
                entity.HasOne(d => d.Office)
                      .WithMany(o => o.Desks)
                      .HasForeignKey(d => d.OfficeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── DeskBooking ────────────────────────────────────────────────────────
            modelBuilder.Entity<DeskBooking>(entity =>
            {
                entity.HasKey(b => b.Id);

                // Enum stocat ca int (Confirmed = 0, Cancelled = 1)
                entity.Property(b => b.Status)
                      .HasConversion<int>();

                // ConfirmedDeskId: setter privat accesibil EF Core prin reflection
                entity.Property(b => b.ConfirmedDeskId)
                      .IsRequired(false);

                // FK NOT NULL → Desks, Restrict (istoricul rezervărilor trebuie păstrat)
                entity.HasOne(b => b.Desk)
                      .WithMany()
                      .HasForeignKey(b => b.DeskId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK NOT NULL → Employees, Restrict
                entity.HasOne(b => b.Employee)
                      .WithMany()
                      .HasForeignKey(b => b.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ── Strat 2: index UNIQUE pe (ConfirmedDeskId, BookingDate) ─────────────
                // MySQL 8 nu suportă indexuri filtrate. Workaround: ConfirmedDeskId
                // este NULL când Status = Cancelled; MySQL permite multiple NULL-uri
                // într-un index UNIQUE, deci rezervările anulate nu participă.
                // Rezervările Confirmed au ConfirmedDeskId = DeskId ⇒ coliziunea
                // pe (desk, dată) e prinsă de baza de date dacă Strat 1 e depășit
                // de o cursă de concurență.
                entity.HasIndex(b => new { b.ConfirmedDeskId, b.BookingDate })
                      .IsUnique()
                      .HasDatabaseName("IX_DeskBookings_ConfirmedDeskId_BookingDate");
            });

            // ── CalendarEvent ───────────────────────────────────────────────────────────
            modelBuilder.Entity<CalendarEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Location)
                      .HasMaxLength(300);

                entity.Property(e => e.MeetingUrl)
                      .HasMaxLength(500);

                entity.Property(e => e.Category)
                      .HasConversion<int>();

                // FK NOT NULL → Employees (creatorul), Restrict
                // Angajatul inactiv nu trebuie să blocheze vizibilitatea evenimentelor istorice
                entity.HasOne(e => e.Creator)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── HRTicket ─────────────────────────────────────────────────────────────
            modelBuilder.Entity<HRTicket>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.TicketNumber)
                      .IsRequired()
                      .HasMaxLength(20);

                // UNIQUE INDEX pe TicketNumber — plasă de siguranță pentru generarea secvențială.
                // MySQL permite un singur rând per valoare, deci o coliziune (concurență) va
                // produce o excepție la nivel DB, nu date duplicate silențioase.
                entity.HasIndex(t => t.TicketNumber)
                      .IsUnique()
                      .HasDatabaseName("IX_HRTickets_TicketNumber");

                entity.Property(t => t.Subject)
                      .IsRequired()
                      .HasMaxLength(255);

                // Conținut lung — stocat ca TEXT (MySQL)
                entity.Property(t => t.Description)
                      .IsRequired()
                      .HasColumnType("TEXT");

                // Enum-urile stocate ca int (implicit EF Core)
                entity.Property(t => t.Category)
                      .HasConversion<int>();

                entity.Property(t => t.Status)
                      .HasConversion<int>();

                // FK NOT NULL → Employees (depunătorul tichetului), Restrict
                // Istoricul tichetelor trebuie păstrat chiar dacă angajatul e dezactivat
                entity.HasOne(t => t.Employee)
                      .WithMany()
                      .HasForeignKey(t => t.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK nullable → Employees (HR/Admin asignat), Restrict
                entity.HasOne(t => t.AssignedTo)
                      .WithMany()
                      .HasForeignKey(t => t.AssignedToId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── LeaveRequest ────────────────────────────────────────────────────────
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(lr => lr.Id);

                entity.Property(lr => lr.LeaveType)
                      .HasConversion<int>();

                entity.Property(lr => lr.Status)
                      .HasConversion<int>();

                entity.Property(lr => lr.TotalDays)
                      .HasPrecision(4, 1);

                entity.Property(lr => lr.Reason)
                      .HasMaxLength(500);

                entity.Property(lr => lr.ManagerComment)
                      .HasMaxLength(500);

                // FK NOT NULL → Employees (cel care face cererea), Restrict
                entity.HasOne(lr => lr.Employee)
                      .WithMany()
                      .HasForeignKey(lr => lr.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK nullable → Employees (managerul care a aprobat/respins), Restrict
                entity.HasOne(lr => lr.ReviewedBy)
                      .WithMany()
                      .HasForeignKey(lr => lr.ReviewedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── LeaveBalance ────────────────────────────────────────────────────────
            modelBuilder.Entity<LeaveBalance>(entity =>
            {
                entity.HasKey(lb => lb.Id);

                entity.Property(lb => lb.LeaveType)
                      .HasConversion<int>();

                entity.Property(lb => lb.AllottedDays)
                      .HasPrecision(5, 1);

                entity.Property(lb => lb.UsedDays)
                      .HasPrecision(5, 1);

                // FK NOT NULL → Employees, Restrict (pentru protecția soft-delete-ului)
                entity.HasOne(lb => lb.Employee)
                      .WithMany()
                      .HasForeignKey(lb => lb.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Constrângere UNIQUE(EmployeeId, Year, LeaveType)
                entity.HasIndex(lb => new { lb.EmployeeId, lb.Year, lb.LeaveType })
                      .IsUnique()
                      .HasDatabaseName("IX_LeaveBalances_EmployeeId_Year_LeaveType");
            });

            // ── Notification ─────────────────────────────────────────────────────────
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Title)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(n => n.NotificationType)
                      .HasMaxLength(80)
                      .IsRequired();

                entity.Property(n => n.RelatedEntityType)
                      .HasMaxLength(80);

                // FK NOT NULL → Employees, Restrict (protecție soft-delete)
                entity.HasOne(n => n.Employee)
                      .WithMany()
                      .HasForeignKey(n => n.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── TrainingCourse ───────────────────────────────────────────────────────
            modelBuilder.Entity<TrainingCourse>(entity =>
            {
                entity.HasKey(tc => tc.Id);

                entity.Property(tc => tc.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(tc => tc.Category)
                      .HasConversion<int>();

                entity.Property(tc => tc.ThumbnailUrl)
                      .HasMaxLength(500);

                // FK NOT NULL → Employees, Restrict
                entity.HasOne(tc => tc.CreatedBy)
                      .WithMany()
                      .HasForeignKey(tc => tc.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── TrainingModule ───────────────────────────────────────────────────────
            modelBuilder.Entity<TrainingModule>(entity =>
            {
                entity.HasKey(tm => tm.Id);

                entity.Property(tm => tm.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(tm => tm.ModuleType)
                      .HasConversion<int>();

                // FK NOT NULL → TrainingCourse, Cascade
                entity.HasOne(tm => tm.Course)
                      .WithMany(tc => tc.Modules)
                      .HasForeignKey(tm => tm.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── CourseEnrollment ─────────────────────────────────────────────────────
            modelBuilder.Entity<CourseEnrollment>(entity =>
            {
                entity.HasKey(ce => ce.Id);

                entity.Property(ce => ce.CertificateUrl)
                      .HasMaxLength(500);

                // FK NOT NULL → TrainingCourse, Restrict
                entity.HasOne(ce => ce.Course)
                      .WithMany(tc => tc.Enrollments)
                      .HasForeignKey(ce => ce.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK NOT NULL → Employees, Restrict
                entity.HasOne(ce => ce.Employee)
                      .WithMany()
                      .HasForeignKey(ce => ce.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Constrângere UNIQUE(CourseId, EmployeeId)
                entity.HasIndex(ce => new { ce.CourseId, ce.EmployeeId })
                      .IsUnique()
                      .HasDatabaseName("IX_CourseEnrollments_CourseId_EmployeeId");
            });
        }
    }
}