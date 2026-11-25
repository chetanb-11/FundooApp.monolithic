namespace Businesslayer.Interface;

using NoteService.Application.Models;

public interface INoteCommands
{
    Task<NoteResponseDto?> CreateAsync(NoteRequestModel noteRequestModel, int userId);
    Task<bool> DeleteAsync(int noteId, int userId);
    Task<bool> UpdateAsync(NoteDomainModel noteDomainModel);
    Task<NoteResponseDto?> ArchiveNote(int noteId, int userId);
    Task<NoteResponseDto?> UnArchiveNote(int noteId, int userId);
    Task<NoteResponseDto?> TrashNote(int noteId, int userId);
    Task<NoteResponseDto?> UnTrashNote(int noteId, int userId);
}