// <copyright file="RedPlanetPremiumFundingApiClient.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Arteva;

using System.Net;
using Flurl.Http;
using System.Text.Json;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Funding;
using UBind.Domain.Helpers;
using UBind.Application.Funding.RedPlanetPremiumFunding.Models;
using Polly.Retry;
using Polly;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

/// <summary>
/// Collection of methods that uses the API endpoints provided by Red Planet - Odyssey API.
/// The public API documentation is outdated. The best way is to use their provided Postman collection instead
///   Reference link : https://uat.redplanetsoftware.com/odyssey-api/doc/#/quotes/post_quotes__number__submit
///   Postman Collection (as of 02/04/2024) : Google Drive H:\Shared drives\uBind.Platform.Workers\UB-11900
/// </summary>
public class RedPlanetPremiumFundingApiClient : IRedPlanetPremiumFundingApiClient
{
    private const string LoginUrl = "/auth/login";
    private const string QuickQuoteUrl = "/quotes/quick-quote?creditor={0}";
    private const string SaveQuoteUrl = "/quotes?save={0}";
    private const string UpdateQuoteUrl = "/quotes/{0}";
    private const string SubmitQuoteUrl = "/quotes/{0}/submit";
    private const string GetQuoteDocumentsUrl = "/quotes/{0}/documents";

    private readonly ICachingResolver cachingResolver;
    private readonly AsyncRetryPolicy invalidTokenRetryPolicy;
    private readonly IRedPlanetFundingConfiguration configuration;
    private TimeSpan timeOutLimit = TimeSpan.FromMinutes(1);

    public RedPlanetPremiumFundingApiClient(
        IRedPlanetFundingConfiguration configuration,
        ICachingResolver cachingResolver,
        ILogger<RedPlanetPremiumFundingApiClient> logger)
    {
        this.configuration = configuration;
        this.cachingResolver = cachingResolver;

        string baseUrl = configuration.BaseUrl.EndsWith('/')
            ? configuration.BaseUrl.TrimEnd('/')
            : configuration.BaseUrl;

        this.ClientUrl = new Uri(baseUrl);

        this.invalidTokenRetryPolicy = Polly.Policy
           .Handle<FlurlHttpException>(r => r.StatusCode == (int)HttpStatusCode.Unauthorized)
           .RetryAsync(3, (exception, retryCount, context) =>
           {
               logger.LogWarning($"Retry {retryCount} for {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
           });
    }

    public delegate Task<Error> TimeoutHandlerDelegate(FlurlHttpTimeoutException ex);

    public delegate Task<string> RequestHandlerDelegate(IFlurlRequest request);

    public Uri ClientUrl { get; }

    /// <inheritdoc/>
    public async Task<QuickQuoteDetail?> QuickQuote(QuickQuoteModel model, CancellationToken cancellationToken)
    {
        var result = await this.QuickQuote(new List<QuickQuoteModel> { model }, cancellationToken);
        return result != null ? result.FirstOrDefault() : null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<QuickQuoteDetail>> QuickQuote(IList<QuickQuoteModel> models, CancellationToken cancellationToken)
    {
        var requestUrl = string.Format(QuickQuoteUrl, this.configuration.CreditorCode);
        var result = await this.SendPostRequest<
            IList<QuickQuoteModel>,
            IEnumerable<QuickQuoteDetail>>(requestUrl, models, cancellationToken);

        var errorMessages = new StringBuilder();
        foreach (var resultItem in result)
        {
            if (!string.IsNullOrEmpty(resultItem.Message))
            {
                errorMessages.AppendLine($"{resultItem.Message}");
            }
        }
        if (errorMessages.Length > 0)
        {
            throw await this.HandleErrorException(
                errorMessages.ToString(), requestUrl, HttpStatusCode.BadRequest);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<QuoteDetail> CreateQuote(CreateUpdateQuoteModel model, bool saveQuote = false, CancellationToken cancellationToken = default)
    {
        var requestUrl = string.Format(SaveQuoteUrl, saveQuote);
        return await this.SendPostRequest<
            CreateUpdateQuoteModel,
            QuoteDetail>(requestUrl, model, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<QuoteDetail> UpdateQuote(CreateUpdateQuoteModel model, string quoteNumber, CancellationToken cancellationToken)
    {
        var requestUrl = string.Format(UpdateQuoteUrl, quoteNumber);
        return await this.SendPostRequest<
            CreateUpdateQuoteModel,
            QuoteDetail>(requestUrl, model, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<QuoteDetail> SubmitQuote(QuoteSubmissionModel model, string quoteNumber, CancellationToken cancellationToken)
    {
        var requestUrl = string.Format(SubmitQuoteUrl, quoteNumber);
        async Task<Error> TimeoutHandler(FlurlHttpTimeoutException ex)
        {
            var dataObject = await this.GetErrorDataObject();
            return Errors.Payment.Funding.RedPlanetPremiumFunding.ApiRequestTimedOut(
                ex.Call.Request.Url.ToString(),
                quoteNumber,
                this.configuration.FundingType,
                dataObject);
        }
        return await this.SendPostRequest<QuoteSubmissionModel, QuoteDetail>(
            requestUrl, model, cancellationToken, timeoutErrorHandler: TimeoutHandler);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<QuoteDocumentModel>> GetQuoteDocuments(string quoteNumber, CancellationToken cancellationToken)
    {
        var requestUrl = string.Format(GetQuoteDocumentsUrl, quoteNumber);
        return await this.SendGetRequest<IEnumerable<QuoteDocumentModel>>(requestUrl, cancellationToken);
    }

    private async Task<TResponse> SendPostRequest<TPayload, TResponse>(
        string requestUrl,
        TPayload requestPayload,
        CancellationToken cancellationToken,
        bool useBearerToken = true,
        TimeoutHandlerDelegate? timeoutErrorHandler = null)
    {
        async Task<string> RequestHandler(IFlurlRequest request)
        {
            return await request.PostJsonAsync(requestPayload, cancellationToken).ReceiveString();
        }

        return await this.invalidTokenRetryPolicy.ExecuteAsync(async () =>
            await this.SendRequest<TResponse>(requestUrl, RequestHandler, useBearerToken, timeoutErrorHandler));
    }

    private async Task<TResponse> SendGetRequest<TResponse>(
        string requestUrl,
        CancellationToken cancellationToken,
        bool useBearerToken = true,
        TimeoutHandlerDelegate? timeoutErrorHandler = null)
    {
        async Task<string> RequestHandler(IFlurlRequest request)
        {
            return await request.GetStringAsync(cancellationToken);
        }

        return await this.invalidTokenRetryPolicy.ExecuteAsync(async () =>
            await this.SendRequest<TResponse>(requestUrl, RequestHandler, useBearerToken, timeoutErrorHandler));
    }

    private async Task<TResponse> SendRequest<TResponse>(
        string requestUrl,
        RequestHandlerDelegate requestHandler,
        bool useBearerToken = true,
        TimeoutHandlerDelegate? timeoutErrorHandler = null)
    {
        try
        {
            var normalizedRequestUrl = requestUrl.StartsWith('/') ? requestUrl.TrimStart('/') : requestUrl;
            var url = $"{this.ClientUrl}/{normalizedRequestUrl}";
            var request = await this.ConfigureRequest(url, useBearerToken);
            var response = await requestHandler(request);
            var result = this.DeserializeApiResponse<TResponse>(response, requestUrl);
            return result;
        }
        catch (FlurlHttpTimeoutException ex)
        {
            throw await this.HandleTimeoutException(timeoutErrorHandler, ex);
        }
        catch (FlurlHttpException ex)
        {
            // let's handle invalid access token separately here
            // we need to removed the cached token and then throw the original error
            // for the retry policy to catch
            if (ex.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                this.RemoveCachedAccessToken();
                throw;
            }
            throw await this.HandleErrorException(ex, requestUrl);
        }
    }

    private TResponse DeserializeApiResponse<TResponse>(string response, string requestUrl)
    {
        var result = JsonSerializer.Deserialize<TResponse>(response, SystemJsonHelper.GetSerializerOptions());
        if (result == null)
        {
            throw new ErrorException(Errors.General.Unexpected($"The premium funding provider \"Arteva\" API client did not return a response for request URL \"{requestUrl}\"."));
        }
        return result;
    }

    private async Task<ErrorException> HandleTimeoutException(
        TimeoutHandlerDelegate? timeoutErrorHandler,
        FlurlHttpTimeoutException ex)
    {
        if (timeoutErrorHandler != null)
        {
            var error = await timeoutErrorHandler(ex);
            if (error != null)
            {
                return new ErrorException(error);
            }
        }
        var dataObject = await this.GetErrorDataObject();
        return new ErrorException(
               Errors.Payment.Funding.RedPlanetPremiumFunding.ApiRequestTimedOut(
                   ex.Call.Request.Url.ToString(), null, this.configuration.FundingType, dataObject));
    }

    private async Task<(string TenantAlias, string ProductAlias)> GetAliases()
    {
        var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(this.configuration.ReleaseContext.TenantId);
        var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(
            this.configuration.ReleaseContext.TenantId,
            this.configuration.ReleaseContext.ProductId);
        return (tenantAlias, productAlias);
    }

    private async Task<IFlurlRequest> ConfigureRequest(string url, bool useBearerToken)
    {
        var flurlRequest = url
            .WithHeader("Content-Type", "application/json")
            .WithTimeout(this.timeOutLimit);
        if (useBearerToken)
        {
            var bearerToken = await this.GetAccessToken();
            flurlRequest = flurlRequest.WithOAuthBearerToken(bearerToken);
        }
        return flurlRequest;
    }

    private async Task<AccessTokenResponse> RequestNewAccessToken()
    {
        var requestPayload = new
        {
            username = this.configuration.Username,
            password = this.configuration.Password,
        };
        AccessTokenResponse result
            = await this.SendPostRequest<object, AccessTokenResponse>(LoginUrl, requestPayload, default, false);
        return result;
    }

    private async Task<string> GetAccessToken()
    {
        FundingAccessTokenKey fundingTokenKey = this.CreateFundingTokenKey();
        var cachedToken = MemoryCachingHelper.Get<string>(fundingTokenKey.ToString());
        if (cachedToken != null)
        {
            return cachedToken;
        }

        var tokenResponse = await this.RequestNewAccessToken();
        if (tokenResponse?.AccessToken == null)
        {
            throw new ErrorException(Errors.Payment.Funding.CouldNotObtainAccessToken());
        }
        MemoryCachingHelper.Upsert(
            fundingTokenKey.ToString(),
            tokenResponse.AccessToken,
            DateTimeOffset.Now.AddSeconds(Convert.ToDouble(tokenResponse.ExpiresIn)));
        return tokenResponse.AccessToken;
    }

    private FundingAccessTokenKey CreateFundingTokenKey()
    {
        var fundingTokenKey = new FundingAccessTokenKey(
            this.configuration.Username,
            this.configuration.ServiceName,
            this.configuration.FundingType.ToString());
        return fundingTokenKey;
    }

    private async Task<ErrorException> HandleErrorException(FlurlHttpException exception, string requestUrl)
    {
        string rawErrorResponse = await exception.GetResponseStringAsync();
        var exceptionStatusCode = exception?.StatusCode ?? (int)HttpStatusCode.BadRequest;
        var httpStatusCode = (HttpStatusCode)exceptionStatusCode;
        var dataObject = await this.GetErrorDataObject();
        return new ErrorException(
            Errors.Payment.Funding.RedPlanetPremiumFunding.ApiRequestFailed(
                requestUrl,
                rawErrorResponse,
                httpStatusCode,
                this.configuration.FundingType,
                dataObject));
    }

    private async Task<ErrorException> HandleErrorException(string rawErrorResponse, string requestUrl, HttpStatusCode httpStatusCode)
    {
        var dataObject = await this.GetErrorDataObject();
        return new ErrorException(
            Errors.Payment.Funding.RedPlanetPremiumFunding.ApiRequestFailed(
                requestUrl,
                rawErrorResponse,
                httpStatusCode,
                this.configuration.FundingType,
                dataObject));
    }

    private async Task<JObject> GetErrorDataObject()
    {
        var aliases = await this.GetAliases();
        return new JObject
        {
            { "username",  this.configuration.Username },
            { "tenantAlias", aliases.TenantAlias },
            { "productAlias", aliases.ProductAlias },
            { "environment", this.configuration.ReleaseContext.Environment.ToString() },
        };
    }

    private void RemoveCachedAccessToken()
    {
        FundingAccessTokenKey fundingTokenKey = this.CreateFundingTokenKey();
        MemoryCachingHelper.Remove(fundingTokenKey.ToString());
    }
}
