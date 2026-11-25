namespace Businesslayer.Interface;

using NoteService.Application.Models;

public interface INoteQueries
{

    Task<IEnumerable<NoteResponseDto>?> GetAllNotesByUser(int userId);
    Task<NoteResponseDto?> GetNoteById(int noteId, int userId);
    Task<List<NoteResponseDto>> GetArchivedNotes(int userId);
    Task<List<NoteResponseDto>> GetTrashedNotes(int userId);
}