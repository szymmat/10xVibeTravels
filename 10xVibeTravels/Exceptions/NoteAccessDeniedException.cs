namespace _10xVibeTravels.Exceptions;

public class NoteAccessDeniedException : Exception
{
    public NoteAccessDeniedException(Guid noteId, string userId)
        : base($"User '{userId}' does not have access to note with ID '{noteId}'.")
    {
    }
    
    public NoteAccessDeniedException(string message)
        : base(message)
    {
    }

    public NoteAccessDeniedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
} 