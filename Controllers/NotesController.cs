using System.Security.Claims;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Exceptions;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _10xVibeTravels.Controllers;

[ApiController]
[Route("api/v1/notes")]
[Authorize] // Require authentication for all actions in this controller
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly ILogger<NotesController> _logger;

    public NotesController(INoteService noteService, ILogger<NotesController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    private string GetUserId()
    {
        // We can be reasonably sure User and NameIdentifier exist due to [Authorize]
         return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        // return "ab801e53-22e2-4f57-bd49-649c90da1a7d";
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedListDto<NoteListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedListDto<NoteListItemDto>>> GetNotes([FromQuery] GetNotesListQuery query)
    {
        // Validation is handled by FluentValidation middleware configured in Program.cs
        var userId = GetUserId();
        var result = await _noteService.GetNotesAsync(userId, query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NoteDto>> CreateNote([FromBody] CreateNoteRequest request)
    {
        // Validation handled by middleware
        var userId = GetUserId();
        var createdNote = await _noteService.CreateNoteAsync(userId, request);

        // Return 201 Created with a Location header pointing to the new resource
        // Requires the GetNote action to be named "GetNote"
        return CreatedAtAction(nameof(GetNote), new { id = createdNote.Id }, createdNote);
    }

    [HttpGet("{id}", Name = "GetNote")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NoteDto>> GetNote(Guid id)
    {
        var userId = GetUserId();
        try
        {
            var note = await _noteService.GetNoteByIdAsync(userId, id);
            // note should not be null here because GetNoteByIdAsync throws if not found
            return Ok(note);
        }
        catch (NoteNotFoundException)
        {
            return NotFound();
        }
        catch (NoteAccessDeniedException)
        {
            return Forbid(); // Return 403 Forbidden if access denied
        }
        // Catch other potential exceptions for 500, although a global handler is better
        catch (Exception ex)
        {
            // Log the exception ex
            _logger.LogError(ex, "An unexpected error occurred while getting note by ID.");
            // Consider logging ex details here
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NoteDto>> UpdateNote(Guid id, [FromBody] UpdateNoteRequest request)
    {
        // Validation handled by middleware
        var userId = GetUserId();
        try
        {
            var updatedNote = await _noteService.UpdateNoteAsync(userId, id, request);
             // updatedNote should not be null here because UpdateNoteAsync throws if not found/denied
            return Ok(updatedNote);
        }
        catch (NoteNotFoundException)
        {
            return NotFound();
        }
        catch (NoteAccessDeniedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            // Log the exception ex
            _logger.LogError(ex, "An unexpected error occurred while updating note.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteNote(Guid id)
    {
        var userId = GetUserId();
        try
        {
            var success = await _noteService.DeleteNoteAsync(userId, id);
            // DeleteNoteAsync throws NoteNotFound or NoteAccessDenied if applicable
            // If it returns, it means the operation was logically successful 
            // (even if DB didn't change anything, e.g., deleting already deleted item - idempotent)
            // The plan specifies 204 on successful deletion attempt.
            return NoContent(); // Return 204 No Content
        }
        catch (NoteNotFoundException)
        {
            return NotFound();
        }
        catch (NoteAccessDeniedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
             // Log the exception ex
            _logger.LogError(ex, "An unexpected error occurred while deleting note.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
} 