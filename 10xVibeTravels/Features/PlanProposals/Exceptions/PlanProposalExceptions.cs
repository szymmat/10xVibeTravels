namespace _10xVibeTravels.Features.PlanProposals.Exceptions;

// Base exception for this feature
public abstract class PlanProposalException : Exception
{
    protected PlanProposalException(string message) : base(message) { }
    
    // Add overload to accept inner exception
    protected PlanProposalException(string message, Exception? innerException) : base(message, innerException) { }
}

// 404 Not Found
public class NoteNotFoundException : PlanProposalException
{
    public NoteNotFoundException(Guid noteId) : base($"Note with ID '{noteId}' not found.") { }
}

// 403 Forbidden
public class NoteForbiddenException : PlanProposalException
{
    public NoteForbiddenException(Guid noteId, string userId) : base($"User '{userId}' is not authorized to access note '{noteId}'.") { }
}

// 400 Bad Request / 422 Unprocessable Entity
public class InvalidDateRangeException : PlanProposalException
{
    public InvalidDateRangeException() : base("End date must be after start date.") { }
}

// 422 Unprocessable Entity
public class BudgetNotAvailableException : PlanProposalException
{
    public BudgetNotAvailableException() : base("Budget could not be determined. Provide a budget in the request or set a default budget in the user profile.") { }
}

// 503 Service Unavailable
public class AiServiceException : PlanProposalException
{
    public AiServiceException(string message, Exception? innerException = null) 
        : base($"Error communicating with the AI service: {message}", innerException) { }
} 