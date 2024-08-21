// <copyright file="ITinyUrlRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories;

using UBind.Domain.Models;

/// <summary>
/// The repository for storing URL tokens and destination URL
/// </summary>
public interface ITinyUrlRepository
{
    void Insert(TinyUrl tinyUrl);

    void SaveChanges();

    /// <summary>
    /// Gets the corresponding destination URL for the given token
    /// </summary>
    /// <param name="token">The unique token to match.</param>
    /// <returns>Destination URL to redirect to</returns>
    Task<TinyUrl?> GetByToken(string token);

    Task<long?> GetMaxSequenceNumber();
}
