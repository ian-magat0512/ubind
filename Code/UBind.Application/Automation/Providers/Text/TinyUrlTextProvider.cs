// <copyright file="TinyUrlTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text;

using Humanizer;
using MorseCode.ITask;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling;
using UBind.Application.Automation;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers;
using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.ValueTypes;

/// <summary>
/// Generates a tiny URL that can be used to redirect to the provided URL.
/// </summary>
/// <remarks>Schema key: TinyUrlText.</remarks>
public class TinyUrlTextProvider : IProvider<Data<string>>
{
    private readonly ITinyUrlService tinyUrlService;
    private readonly IBaseUrlResolver baseUrlResolver;
    private readonly ICachingResolver cachingResolver;
    private IProvider<Data<string>> redirectUrl;
    private string appBaseUrl;

    public TinyUrlTextProvider(
        IProvider<Data<string>> redirectUrl,
        ITinyUrlService tinyUrlService,
        IBaseUrlResolver baseUrlResolver,
        ICachingResolver cachingResolver)
    {
        this.redirectUrl = redirectUrl;
        this.tinyUrlService = tinyUrlService;
        this.cachingResolver = cachingResolver;
        this.baseUrlResolver = baseUrlResolver;
    }

    public string SchemaReferenceKey => "tinyUrlText";

    /// <inheritdoc/>
    public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
    {
        using (MiniProfiler.Current.Step($"{this.GetType().Name}.{nameof(this.Resolve)}"))
        {
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            this.appBaseUrl = this.baseUrlResolver.GetBaseUrl(tenant).Trim('/').ToLower();
            var environment = providerContext.AutomationData.System.Environment;
            string url = (await this.redirectUrl.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var redirectUrl = await this.GetValidRedirectUrlToPersistOrThrow(
                url,
                async (invalidUrl) => await this.GetErrorData(providerContext, invalidUrl));
            var tinyUrl = await this.tinyUrlService.GenerateAndPersistUrl(
                tenantId,
                environment,
                redirectUrl);

            if (!string.IsNullOrWhiteSpace(tinyUrl))
            {
                return ProviderResult<Data<string>>.Success(new Data<string>(tinyUrl));
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, redirectUrl.ToString().Truncate(80, "..."));
            throw new ErrorException(Errors.Automation.ValueResolutionError(this.SchemaReferenceKey, errorData));
        }
    }

    private async Task<string> GetValidRedirectUrlToPersistOrThrow(string redirectUrl, Func<string, Task<(JObject, List<string>)>> getErrorDataCallback)
    {
        string absoluteUrl = await this.GetValidAbsoluteUrlOrThrow(redirectUrl, getErrorDataCallback);
        return absoluteUrl.ToLower().StartsWith(this.appBaseUrl.ToLower()) ? new Uri(absoluteUrl).PathAndQuery : absoluteUrl;
    }

    private async Task<string> GetValidAbsoluteUrlOrThrow(string redirectUrl, Func<string, Task<(JObject, List<string>)>> getErrorDataCallback)
    {
        var absoluteUrl = redirectUrl;
        if (string.IsNullOrEmpty(redirectUrl))
        {
            var errorData = await getErrorDataCallback(null);
            throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                "redirectUrl",
                this.SchemaReferenceKey,
                errorData.Item1,
                errorData.Item2));
        }

        if (redirectUrl.StartsWith("//"))
        {
            await this.ThrowInvalidUrl(redirectUrl, getErrorDataCallback);
        }
        else if (redirectUrl.StartsWith("/"))
        {
            // means it's a relative URL
            absoluteUrl = $"{this.appBaseUrl}{redirectUrl}";
        }
        else if (!redirectUrl.StartsWith("http"))
        {
            absoluteUrl = $"https://{redirectUrl}";
        }

        try
        {
            var url = new Url(absoluteUrl);
        }
        catch (ErrorException)
        {
            await this.ThrowInvalidUrl(redirectUrl, getErrorDataCallback);
        }

        return absoluteUrl;
    }

    private async Task ThrowInvalidUrl(string invalidUrl, Func<string, Task<(JObject, List<string>)>> getErrorDataCallback)
    {
        var errorDetails = await getErrorDataCallback(invalidUrl);
        throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(
            this.SchemaReferenceKey,
            "redirectUrl",
            null,
            errorDetails.Item1,
            errorDetails.Item2,
            $"The \"redirectUrl\" parameter must contain a value that is a valid URL. "));
    }

    private async Task<(JObject, List<string>)> GetErrorData(IProviderContext providerContext, string redirectUrl)
    {
        var errorKeys = new List<string>
        {
            ErrorDataKey.TenantAlias,
            ErrorDataKey.ProductAlias,
            ErrorDataKey.Environment,
            ErrorDataKey.Feature,
        };

        var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
        var additionalDetails = new List<string>();
        foreach (var key in errorKeys)
        {
            var value = errorData.GetValue(key);
            if (key.Equals(ErrorDataKey.Environment) || key.Equals(ErrorDataKey.Feature))
            {
                value = value?.ToString()?.Titleize();
            }
            additionalDetails.Add($"{key.Titleize()}: {errorData.GetValue(key)}");
        }

        if (redirectUrl != null)
        {
            additionalDetails.Add($"Redirect URL: {redirectUrl.ToString().Truncate(80, "...")}");
            errorData.Add(ErrorDataKey.ValueToParse, redirectUrl.ToString().Truncate(80, "..."));
        }

        return (errorData, additionalDetails);
    }
}
