// <copyright file="IRedisHashSetRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Domain.Repositories.Redis;

using System.Threading.Tasks;

/// <summary>
/// Interface for redis hashset repository.
/// </summary>
public interface IRedisHashSetRepository<T>
{
    /// <summary>
    /// Adds a new value to the hashset.
    /// </summary>
    Task Add(T value);

    /// <summary>
    /// Checks if the hashset has the specified value.
    /// </summary>
    Task<bool> Contains(T value);

    /// <summary>
    /// Deletes the hashset, along with its values.
    /// </summary>
    Task DeleteSet();

    /// <summary>
    /// Gets the total values stored in the hashset.
    /// </summary>
    Task<long> GetCount();
}
