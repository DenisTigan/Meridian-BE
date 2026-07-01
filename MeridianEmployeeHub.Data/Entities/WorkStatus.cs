namespace MeridianEmployeeHub.Data.Entities
{
    // Enum pentru statusul de lucru al unui angajat.
    // Stocat ca int în DB (0 = Office, 1 = Remote, 2 = OnLeave).
    public enum WorkStatus
    {
        Office = 0,
        Remote = 1,
        OnLeave = 2
    }
}
