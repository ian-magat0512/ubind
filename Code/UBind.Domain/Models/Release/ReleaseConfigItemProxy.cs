// <copyright file="ReleaseConfigItemProxy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models.Release;

using NodaTime;
using UBind.Domain.Extensions;
using UBind.Domain.Repositories;

/// <summary>
/// Encapsulates a configuration item on a release, which is stored in ReleaseDetails as
/// multiple separate top level properties.
/// This proxy allows us to represent that configuration item as a single object.
/// </summary>
public class ReleaseConfigItemProxy<T> : IReleaseConfigItemProxy
{
    private readonly ReleaseDetails releaseDetails;
    private readonly Func<ReleaseDetails, T?> valueGetter;
    private readonly Action<ReleaseDetails, T?> valueSetter;
    private readonly Func<ReleaseDetails, Instant?> timestampGetter;
    private readonly Action<ReleaseDetails, Instant?> timestampSetter;
    private readonly Func<Task>? onValueChangedAsync;

    public ReleaseConfigItemProxy(
        ReleaseDetails releaseDetails,
        Func<ReleaseDetails, T?> getValue,
        Action<ReleaseDetails, T?> setValue,
        Func<ReleaseDetails, Instant?> getTimestamp,
        Action<ReleaseDetails, Instant?> setTimestamp,
        string filePath,
        Func<Task>? onValueChangedAsync = null)
    {
        this.releaseDetails = releaseDetails;
        this.valueGetter = getValue;
        this.valueSetter = setValue;
        this.timestampGetter = getTimestamp;
        this.timestampSetter = setTimestamp;
        this.FilePath = filePath;
        this.onValueChangedAsync = onValueChangedAsync;
    }

    public T? Value
    {
        get => this.valueGetter(this.releaseDetails);
        set => this.valueSetter(this.releaseDetails, value);
    }

    public Instant? Timestamp
    {
        get => this.timestampGetter(this.releaseDetails);
        set => this.timestampSetter(this.releaseDetails, value);
    }

    public string FilePath { get; }

    public async Task CheckAndUpdate(IFilesystemFileRepository fileRepository, string accessToken, IClock clock)
    {
        var fileInfo = await fileRepository.GetFileInfo(this.FilePath, accessToken);
        bool updated = false;
        if (fileInfo == null)
        {
            if (this.Value != null)
            {
                this.Value = default(T);
                updated = true;
                this.Timestamp = clock.Now();
            }
        }
        else if (this.Timestamp == null || fileInfo.LastModifiedTimestamp != this.Timestamp)
        {
            if (typeof(T) == typeof(string))
            {
                this.Value = (T)(object)await fileRepository.GetFileStringContents(
                    this.FilePath,
                    accessToken);
            }
            else if (typeof(T) == typeof(byte[]))
            {
                this.Value = (T)(object)await fileRepository.GetFileContents(
                    this.FilePath,
                    accessToken);
            }

            this.Timestamp = fileInfo.LastModifiedTimestamp;
            updated = true;
        }

        if (updated && this.onValueChangedAsync != null)
        {
            await this.onValueChangedAsync();
        }
    }
}
