// <copyright file="IReleaseConfigItemProxy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models.Release;

using NodaTime;
using UBind.Domain.Repositories;

public interface IReleaseConfigItemProxy
{
    /// <summary>
    /// Checks the timestamp of the file and updates the ReleaseDetails proprties if needed.
    /// </summary>
    Task CheckAndUpdate(IFilesystemFileRepository fileRepository, string accessToken, IClock clock);
}
