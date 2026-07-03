namespace MeridianEmployeeHub.Data.Entities
{
    // Enum pentru fazele de onboarding ale unui angajat nou.
    // DayOne   — prima zi (sarcini imediate: parolă, profil, politici)
    // WeekOne  — prima săptămână (integrare în echipă)
    // FirstMonth — prima lună (obiective de rol)
    public enum OnboardingPhase
    {
        DayOne = 0,
        WeekOne = 1,
        FirstMonth = 2
    }
}
