using Application.Models.System;
using Application.Requests.Integrations;
using Domain.DatabaseEntities.Integrations;

namespace Application.Mappers.Integrations;

public static class FileStorageRecordHelpers
{
    public static FileStorageRecordSlim ToSlim(this FileStorageRecordDb record)
    {
        return new FileStorageRecordSlim
        {
            Id = record.Id,
            LinkedType = record.LinkedType,
            LinkedId = record.LinkedId,
            FriendlyName = record.FriendlyName,
            Filename = record.Filename,
            Description = record.Description,
            HashSha256 = record.HashSha256,
            Version = record.Version,
            CreatedBy = record.CreatedBy,
            CreatedOn = record.CreatedOn,
            LastModifiedBy = record.LastModifiedBy,
            LastModifiedOn = record.LastModifiedOn,
            IsDeleted = record.IsDeleted,
            DeletedOn = record.DeletedOn
        };
    }

    public static IEnumerable<FileStorageRecordSlim> ToSlims(this IEnumerable<FileStorageRecordDb> records)
    {
        return records.Select(ToSlim);
    }

    public static FileStorageRecordCreate ToCreate(this FileStorageRecordCreateRequest request)
    {
        return new FileStorageRecordCreate
        {
            LinkedType = request.LinkedType,
            LinkedId = request.LinkedId,
            FriendlyName = request.FriendlyName,
            Filename = request.Filename,
            Description = request.Description,
            Version = request.Version
        };
    }

    public static FileStorageRecordUpdate ToUpdate(this FileStorageRecordUpdateRequest request)
    {
        return new FileStorageRecordUpdate
        {
            Id = request.Id,
            LinkedType = null,
            LinkedId = null,
            FriendlyName = request.FriendlyName,
            Filename = request.Filename,
            Description = request.Description,
            HashSha256 = null,
            Version = request.Version,
            CreatedBy = null,
            CreatedOn = null,
            LastModifiedBy = null,
            LastModifiedOn = null,
            IsDeleted = null,
            DeletedOn = null
        };
    }
}