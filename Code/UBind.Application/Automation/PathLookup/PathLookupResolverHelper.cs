// <copyright file="PathLookupResolverHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.PathLookup;

using Newtonsoft.Json.Linq;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers;
using UBind.Domain;
using UBind.Domain.Exceptions;

/// <summary>
/// This class is a helper class for the path lookup providers.
/// It resolves the values for the path lookup providers and throws errors
/// if the values are not found, null or the type is not matching.
/// Else it returns the default value.
/// </summary>
public class PathLookupResolverHelper
{
    public static async Task ResolveRaiseErrorIfNotFound(
        IProvider<Data<bool>>? raiseErrorIfNotFoundProvider,
        IProviderContext providerContext,
        string schemaReferenceKey,
        IProviderResult<IData> lookupResult)
    {
        var raiseErrorIfNotFound = (await raiseErrorIfNotFoundProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false;
        if (lookupResult.IsFailure
            && lookupResult.Error.Code.Equals(Errors.Automation.PathNotFound(schemaReferenceKey, null!, null!, null!, null!).Code)
            && (raiseErrorIfNotFoundProvider == null
                || raiseErrorIfNotFound))
        {
            // Do not raise error if the lookup is for objectPathLookupListObject in production environment
            // this is to prevent the error from being thrown because we need to ensure the migration of quotes to new release it should be done first.
            // it should be remove once the migration of quotes to new release is done.
            // will be remove on UB-12013
            if (schemaReferenceKey == "objectPathLookupListObject"
                && providerContext.AutomationData.System.Environment == DeploymentEnvironment.Production
                && raiseErrorIfNotFoundProvider == null)
            {
                return;
            }

            JObject errorData = await providerContext.GetDebugContextForProviders(schemaReferenceKey);
            errorData.Add(ErrorDataKey.QueryPath, lookupResult.Error.Data?.GetValue(ErrorDataKey.QueryPath));
            throw new ErrorException(Errors.Automation.PathQueryValueNotFound(schemaReferenceKey, errorData));
        }
        else if (lookupResult.IsFailure
            && !lookupResult.Error.Code.Equals(Errors.Automation.PathNotFound(schemaReferenceKey, null!, null!, null!, null!).Code))
        {
            lookupResult.GetValueOrThrowIfFailed();
        }
    }

    public static async Task<IData?> ResolveValueIfNotFound(
        IProvider<IData> valueIfNotFoundProvider,
        IProviderContext providerContext,
        string schemaReferenceKey,
        IProviderResult<IData> lookupResult)
    {
        if (valueIfNotFoundProvider != null
            && lookupResult.IsFailure
            && lookupResult.Error.Code.Equals(Errors.Automation.PathNotFound(schemaReferenceKey, null!, null!, null!, null!).Code))
        {
            return (await valueIfNotFoundProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
        }

        return null;
    }

    public static async Task ResolveRaiseErrorIfNull(
        IProvider<Data<bool>>? raiseErrorIfNullProvider,
        IProviderContext providerContext,
        string schemaReferenceKey,
        IData? lookupData)
    {
        var raiseErrorIfNull = (await raiseErrorIfNullProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false;
        if (raiseErrorIfNull && lookupData == null)
        {
            JObject errorData = await providerContext.GetDebugContextForProviders(schemaReferenceKey);
            throw new ErrorException(Errors.Automation.PathQueryValueIfNull(schemaReferenceKey, errorData));
        }
    }

    public static async Task<IData?> ResolveValueIfNull(
        IProvider<IData>? valueIfNullProvider,
        IProviderContext providerContext,
        IData? lookupData)
    {
        if (valueIfNullProvider != null && lookupData == null)
        {
            return (await valueIfNullProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
        }

        return null;
    }

    public static async Task<IData?> ResolveDefaultValue(
        IProvider<IData>? defaultValueProvider,
        IProviderContext providerContext,
        IData? lookupData)
    {
        if (defaultValueProvider != null && lookupData == null)
        {
            return (await defaultValueProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
        }

        return lookupData;
    }

    public static async Task ResolveRaiseErrorIfTypeMismatch(
        IProvider<Data<bool>>? raiseErrorIfTypeMismatchProvider,
        IProviderContext providerContext,
        IData lookupData,
        string resultValueType,
        string supportedValueType,
        string schemaReferenceKey,
        Exception? exception = null)
    {
        var raiseErrorIfTypeMismatch = (await raiseErrorIfTypeMismatchProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? true;
        if (raiseErrorIfTypeMismatch)
        {
            JObject errorData = await providerContext.GetDebugContextForProviders(schemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, lookupData.ToString());
            if (exception != null)
            {
                errorData.Add(ErrorDataKey.ErrorMessage, exception.Message);
            }

            throw new ErrorException(Errors.Automation.PathQueryValueInvalidType(schemaReferenceKey, resultValueType, supportedValueType, errorData));
        }
    }

    public static async Task<IData?> ResolveValueOrThrowIfNotFound(
        IProvider<Data<bool>>? raiseErrorIfNotFoundProvider,
        IProvider<IData>? valueIfNotFoundProvider,
        IProvider<IData>? defaultValueProvider,
        IProviderContext providerContext,
        string schemaReferenceKey,
        IProviderResult<IData> lookupResult)
    {
        if (valueIfNotFoundProvider == null && defaultValueProvider == null)
        {
            await ResolveRaiseErrorIfNotFound(raiseErrorIfNotFoundProvider, providerContext, schemaReferenceKey, lookupResult);
        }

        if (valueIfNotFoundProvider != null)
        {
            return await ResolveValueIfNotFound(valueIfNotFoundProvider, providerContext, schemaReferenceKey, lookupResult);
        }
        else
        {
            return await ResolveDefaultValue(defaultValueProvider, providerContext, lookupResult.Value);
        }
    }

    public static async Task<IData?> ResolveValueOrThrowIfNull(
        IProvider<Data<bool>>? raiseErrorIfNullProvider,
        IProvider<IData>? valueIfNullProvider,
        IProvider<IData>? defaultValueProvider,
        IProviderContext providerContext,
        string schemaReferenceKey,
        IData? lookupData)
    {
        if (valueIfNullProvider != null)
        {
            return await ResolveValueIfNull(valueIfNullProvider, providerContext, lookupData);
        }
        else
        {
            await ResolveRaiseErrorIfNull(raiseErrorIfNullProvider, providerContext, schemaReferenceKey, lookupData);
        }

        return await ResolveDefaultValue(defaultValueProvider, providerContext, lookupData);
    }

    public static async Task<IData?> ResolveValueOrThrowIfTypeMismatch(
        IProvider<Data<bool>> raiseErrorIfTypeMismatch,
        IProvider<IData>? valueIfTypeMismatch,
        IProvider<IData>? defaultValueProvider,
        IProviderContext providerContext,
        string resultValueType,
        string supportedValueType,
        IData lookupData,
        string schemaReferenceKey,
        Exception? exception = null)
    {
        if (valueIfTypeMismatch == null && defaultValueProvider == null)
        {
            await ResolveRaiseErrorIfTypeMismatch(
                raiseErrorIfTypeMismatch,
                providerContext,
                lookupData,
                resultValueType,
                supportedValueType,
                schemaReferenceKey,
                exception);
        }

        if (valueIfTypeMismatch != null)
        {
            return (await valueIfTypeMismatch.Resolve(providerContext)).GetValueOrThrowIfFailed();
        }
        else
        {
            return await ResolveDefaultValue(defaultValueProvider, providerContext, null);
        }
    }
}
