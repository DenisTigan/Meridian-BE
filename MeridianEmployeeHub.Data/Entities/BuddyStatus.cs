namespace MeridianEmployeeHub.Data.Entities
{
    // Enum pentru starea unui BuddyAssignment.
    // Active    — angajatul nou are un buddy activ asignat
    // Completed — relația de buddy a fost finalizată (angajatul s-a integrat)
    public enum BuddyStatus
    {
        Active = 0,
        Completed = 1
    }
}
