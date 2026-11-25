using System.Text.Json;
using Businesslayer.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NoteService.Application.Models;
using NoteService.Domain.Entities;
using NoteService.Infrastructure.Interfaces;

namespace Businesslayer.Service;

public class NoteCommands(INoteRl noteRl, IDistributedCache cache, ILogger<NoteCommands> logger) : INoteCommands
{
    private readonly INoteRl _noteRl = noteRl;

    private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(600)
    };

    public async Task<NoteResponseDto?> CreateAsync(NoteRequestModel noteRequestModel, int userId)
    {
        if (noteRequestModel == null || userId <= 0)
        {
            logger.LogWarning("Invalid note request or UserId {UserId}", userId);
            return null;
        }

        var note = new NoteEntity
        {
            UserId = userId,
            Title = noteRequestModel.Title,
            Description = noteRequestModel.Description
        };

        var createdNote = await _noteRl.CreateAsync(note);
        if (createdNote == null) return null;

        await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated notes list cache for user {UserId}", userId);

        return new NoteResponseDto
        {
            Id = createdNote.Id,
            Title = createdNote.Title,
            Description = createdNote.Description,
            IsArchived = createdNote.IsArchived,
            IsTrashed = createdNote.IsTrashed
        };
    }

    public async Task<bool> DeleteAsync(int noteId, int userId)
    {
        if (noteId <= 0 || userId <= 0)
        {
            logger.LogWarning("Invalid NoteId {NoteId} or UserId {UserId}", noteId, userId);
            return false;
        }

        var isDeleted = await _noteRl.DeleteAsync(noteId, userId);
        if (!isDeleted) return isDeleted;

        await SafeRemoveCacheAsync($"Notes:User:{userId}:{noteId}");
        await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);

        return isDeleted;
    }

    public async Task<bool> UpdateAsync(NoteDomainModel? noteDomainModel)
    {
        if (noteDomainModel == null || noteDomainModel.Id <= 0 || noteDomainModel.UserId <= 0)
        {
            logger.LogWarning("Invalid NoteDomainModel: NoteId={NoteId}, UserId={UserId}", noteDomainModel?.Id,
                noteDomainModel?.UserId);
            return false;
        }

        var noteId = noteDomainModel.Id;
        var userId = noteDomainModel.UserId;

        var noteEntity = new NoteEntity
        {
            Id = noteDomainModel.Id,
            Title = noteDomainModel.Title,
            Description = noteDomainModel.Description,
            UserId = noteDomainModel.UserId
        };

        var result = await _noteRl.UpdateAsync(noteEntity);
        if (result)
        {
            await SafeRemoveCacheAsync($"Notes:User:{userId}:{noteId}");
            await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
            logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);
        }

        return result;
    }

    public Task<NoteResponseDto?> ArchiveNote(int noteId, int userId)
        => SetArchived(noteId, userId, true);

    public Task<NoteResponseDto?> UnArchiveNote(int noteId, int userId)
        => SetArchived(noteId, userId, false);

    private async Task<NoteResponseDto?> SetArchived(int noteId, int userId, bool isArchived)
    {
        if (noteId <= 0 || userId <= 0)
        {
            logger.LogWarning("Invalid NoteId {NoteId} or UserId {UserId}", noteId, userId);
            return null;
        }

        var note = await _noteRl.SetArchived(noteId, userId, isArchived);
        if (note is null) return null;

        await SafeRemoveCacheAsync($"Notes:User:{userId}:{noteId}");
        await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);

        return ToDto(note);
    }

    public Task<NoteResponseDto?> TrashNote(int noteId, int userId)
        => SetTrashed(noteId, userId, true);

    public Task<NoteResponseDto?> UnTrashNote(int noteId, int userId)
        => SetTrashed(noteId, userId, false);

    private async Task<NoteResponseDto?> SetTrashed(int noteId, int userId, bool isTrashed)
    {
        if (noteId <= 0 || userId <= 0)
        {
            logger.LogWarning("Invalid NoteId {NoteId} or UserId {UserId}", noteId, userId);
            return null;
        }

        var note = await _noteRl.SetTrashed(noteId, userId, isTrashed);
        if (note is null) return null;

        await SafeRemoveCacheAsync($"Notes:User:{userId}:{noteId}");
        await SafeRemoveCacheAsync($"Notes:User:{userId}:All");
        logger.LogInformation("Invalidated cache for note {NoteId} and user {UserId}", noteId, userId);

        return ToDto(note);
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