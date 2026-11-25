using Businesslayer.Interface;

namespace Businesslayer.Service;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NoteService.Application.Models;
using NoteService.Domain.Entities;
using NoteService.Infrastructure.Interfaces;

public class NoteQueries(INoteRl noteRl, IDistributedCache cache, ILogger<NoteQueries> logger) : INoteQueries
{
    private readonly INoteRl _noteRl = noteRl;

    private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(600)
    };

    public async Task<IEnumerable<NoteResponseDto>?> GetAllNotesByUser(int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("Invalid UserId {UserId}", userId);
            return null;
        }

        var key = $"Notes:User:{userId}:All";
        var cached = await SafeGetStringAsync(key);
        if (cached is not null)
        {
            logger.LogInformation("All notes found in Redis cache with key {Key}", key);
            return JsonSerializer.Deserialize<IEnumerable<NoteResponseDto>>(cached);
        }

        var noteEntities = await _noteRl.GetAllNotesByUser(userId);
        var notes = noteEntities.Select(ToDto).ToList();

        logger.LogInformation("Cached {Count} notes for user {UserId}", notes.Count, userId);
        await SafeSetStringAsync(key, JsonSerializer.Serialize(notes), DefaultCacheOptions);

        return notes;
    }

    public async Task<NoteResponseDto?> GetNoteById(int noteId, int userId)
    {
        if (userId <= 0 || noteId <= 0)
        {
            logger.LogWarning("Invalid NoteId {NoteId} or UserId {UserId}", noteId, userId);
            return null;
        }

        var key = $"Notes:User:{userId}:{noteId}";
        var cached = await SafeGetStringAsync(key);
        if (cached is not null)
        {
            logger.LogInformation("Note found in Redis cache with key {Key}", key);
            return JsonSerializer.Deserialize<NoteResponseDto>(cached);
        }

        var noteEntity = await _noteRl.GetNoteById(noteId, userId);
        if (noteEntity == null) return null;

        var note = ToDto(noteEntity);

        logger.LogInformation("Cached note with key {Key}", key);
        await SafeSetStringAsync(key, JsonSerializer.Serialize(note), DefaultCacheOptions);
        return note;
    }

    public async Task<List<NoteResponseDto>> GetArchivedNotes(int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("Invalid UserId {UserId}", userId);
            return [];
        }

        var note = await _noteRl.GetArchived(userId);
        return note.Select(ToDto).ToList();
    }

    public async Task<List<NoteResponseDto>> GetTrashedNotes(int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("Invalid UserId {UserId}", userId);
            return [];
        }

        var notes = await _noteRl.GetTrashed(userId);
        return notes.Select(ToDto).ToList();
    }

    private async Task SafeRemoveCacheAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache unavailable. Could not remove key {Key}", key);
        }
    }

    private async Task<string?> SafeGetStringAsync(string key)
    {
        try
        {
            return await cache.GetStringAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache unavailable. Could not read key {Key}", key);
            return null;
        }
    }

    private async Task SafeSetStringAsync(string key, string value, DistributedCacheEntryOptions options)
    {
        try
        {
            await cache.SetStringAsync(key, value, options);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache unavailable. Could not set key {Key}", key);
        }
    }

    private static NoteResponseDto ToDto(NoteEntity n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Description = n.Description,
        IsArchived = n.IsArchived,
        IsTrashed = n.IsTrashed
    };
}