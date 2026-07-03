namespace MeridianEmployeeHub.Services.Exceptions
{
    // Excepție custom pentru conflicte de stare (HTTP 409 Conflict).
    // Folosită când o resursă există deja sau starea curentă interzice operația.
    // Exemplu: un angajat are deja un BuddyAssignment activ.
    // Gestionată de ExceptionHandlingMiddleware → StatusCode 409.
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
