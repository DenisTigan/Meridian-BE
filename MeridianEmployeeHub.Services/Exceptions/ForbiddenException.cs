namespace MeridianEmployeeHub.Services.Exceptions
{
    // Exceptie custom pentru cazuri de ownership check (403 Forbidden).
    // Numita "ForbiddenException" pentru a evita coliziunea cu System.UnauthorizedAccessException.
    // Exemplu de utilizare:
    //   if (currentUser.Id != resource.EmployeeId && !currentUser.IsHROrAdmin())
    //       throw new ForbiddenException("Access denied.");
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
