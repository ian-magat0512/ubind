// <copyright file="ProviderResultExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Extensions;

using UBind.Application.Automation.Providers;
using UBind.Domain.Exceptions;

/// <summary>
/// This extension class is used to throw an error if the provider result is a failure.
/// So the error can be caught and handled in the calling method.
/// </summary>
public static class ProviderResultExtensions
{
    public static TValue GetValueOrThrowIfFailed<TValue>(this IProviderResult<TValue> providerResult)
    {
        if (providerResult.IsFailure)
        {
            throw new ErrorException(providerResult.Error);
        }

        return providerResult.Value;
    }
}
