namespace MeridianEmployeeHub.Data.Entities
{
    // Această clasă conține proprietățile comune pe care le va avea ORICE tabel din baza de date.
    // În loc să scriem Id, CreatedAt, UpdatedAt în fiecare clasă, le scriem aici și le moștenim.
    public abstract class BaseEntity
    {
        public int Id { get; set; } // Cheia primară (Primary Key)
        public DateTime CreatedAt { get; set; } // Data la care a fost creată înregistrarea
        public DateTime UpdatedAt { get; set; } // Data la care a fost actualizată ultima dată

        // FK nullable către Employees — populat automat din HttpContext după sesiunea de Auth
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}