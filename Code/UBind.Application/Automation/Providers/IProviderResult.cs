// <copyright file="IProviderResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers;

/// <summary>
/// This class is used to return a result from a provider.
/// It will contain either a value or an error before it throws an exception.
/// Since the Result library for C# is not applicable for out scenario isnce the IProvider interface was need to be out the TDataValue generic type.
/// Some callers of IProvider interface should be not to cast the result to the TDataValue type.
/// </summary>
/// <typeparam name="TDataValue">The generic type of the provider</typeparam>
public interface IProviderResult<out TDataValue>
{
    bool IsSuccess { get; }

    bool IsFailure { get; }

    TDataValue Value { get; }

    Domain.Error Error { get; }
}
