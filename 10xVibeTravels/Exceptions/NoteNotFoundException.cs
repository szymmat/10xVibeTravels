using System;

namespace _10xVibeTravels.Exceptions;

public class NoteNotFoundException : Exception
{
    public NoteNotFoundException(Guid noteId) 
        : base($"Note with ID '{noteId}' was not found.")
    {
    }

    public NoteNotFoundException(string message) 
        : base(message)
    {
    }

    public NoteNotFoundException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
} 