// <copyright file="IProviderExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers;

using UBind.Application.Automation.Extensions;

/// <summary>
/// Extension methods to easily use providers.
/// </summary>
public static class IProviderExtensions
{
    /// <summary>
    /// Allows you to attempt to a resolve a value from a provider, even if the provider itself is null.
    /// It makes it easier to use providers in a null-safe way, without having to do a specific null check on the
    /// provider first.
    /// Use this in situations where you are comfortable having an exception throw if the provider fails with an error.
    /// If you need to handle the error, use <see cref="ResolveResultIfNotNull{TResult}"/>.
    /// </summary>
    public static async Task<TValue?> ResolveValueIfNotNull<TValue>(
        this IProvider<TValue>? provider,
        IProviderContext providerContext)
    {
        if (provider == null)
        {
            return default;
        }

        var providerResult = await provider.Resolve(providerContext);
        var result = providerResult.GetValueOrThrowIfFailed();
        return result;
    }

    /// <summary>
    /// Allows you to attempt to a resolve a result froma provider, even if the provider itself is null.
    /// It makes it easier to use providers in a null-safe way, without having to do a specific null check on the
    /// provider first.
    /// Use this in situations where you want the result so you can check for failure and potentially access the error.
    /// If you'd rather the error be thrown as an exception automatically, use <see cref="ResolveValueIfNotNull{TResult}"/>.
    /// </summary>
    public static async Task<IProviderResult<TValue>?> ResolveResultIfNotNull<TValue>(
        this IProvider<TValue>? provider,
        IProviderContext providerContext)
    {
        if (provider == null)
        {
            return default;
        }

        var providerResult = await provider.Resolve(providerContext);
        return providerResult;
    }
}
