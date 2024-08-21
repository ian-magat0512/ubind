// <copyright file="HttpRequestAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Flurl.Http;
    using Microsoft.Extensions.Primitives;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// Represents an action of type HttpRequestAction.
    /// </summary>
    public class HttpRequestAction : Action
    {
        private readonly IClock clock;
        private int requestTimeoutInMilliseconds = 600;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestAction"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="alias">The alias of the action.</param>
        /// <param name="description">The action description.</param>
        /// <param name="asynchronous">The asynchronous to be used.</param>
        /// <param name="runCondition">An optional condition.</param>
        /// <param name="beforeRunErrorConditions">The validation rules before the action.</param>
        /// <param name="afterRunErrorConditions">The validation rules after the action.</param>
        /// <param name="onErrorActions">A list of actions to run if an error is encountered on processing.</param>
        /// <param name="httpRequest">The http request definition that the action will result in.</param>
        /// <param name="clock">The clock for telling time.</param>
        public HttpRequestAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunErrorConditions,
            IEnumerable<ErrorCondition> afterRunErrorConditions,
            IEnumerable<Action> onErrorActions,
            HttpRequestConfiguration httpRequest,
            IClock clock)
            : base(name, alias, description, asynchronous, runCondition, beforeRunErrorConditions, afterRunErrorConditions, onErrorActions)
        {
            this.HttpRequest = httpRequest;
            this.clock = clock;
        }

        /// <summary>
        /// Gets the httpRequest that the action will result in.
        /// </summary>
        public HttpRequestConfiguration HttpRequest { get; }

        /// <inheritdoc/>
        public override ActionData CreateActionData() => new HttpRequestActionData(this.Name, this.Alias, this.clock);

        /// <summary>
        /// Fires an http request.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="actionData">The action data to be updated.</param>
        /// <returns>An awaitable task.</returns>
        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            var httpActionData = (HttpRequestActionData)actionData;
            using (MiniProfiler.Current.Step(nameof(HttpRequestAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                var request = await this.HttpRequest.GenerateRequest(providerContext);
                httpActionData.HttpRequest = request;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.DefaultConnectionLimit = 9999;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12;

                IFlurlRequest flurlRequest = await this.GenerateFlurlRequest(request, providerContext);
                HttpResponseMessage? response = null;
                try
                {
                    using (MiniProfiler.Current.Step(nameof(HttpRequestAction)
                        + "." + nameof(this.Execute) + ".FlurlRequest.SendAsync"))
                    {
                        IFlurlResponse flurlResponse = await flurlRequest.SendAsync(
                                new HttpMethod(request.HttpVerb),
                                await HttpContentBuilder.Build(request.Content, request.ContentType, request.CharacterSet, providerContext.AutomationData),
                                providerContext.CancellationToken);
                        response = flurlResponse.ResponseMessage;
                    }

                    await this.ProcessResponse(response, httpActionData);
                }
                catch (Exception ex) when (ex is FlurlHttpException || ex is UriFormatException)
                {
                    var error = await this.ProcessException(ex, providerContext, httpActionData, request.ClientCertificateData);

                    return Result.Failure<Void, Domain.Error>(error);
                }
                catch (ErrorException ex)
                {
                    return Result.Failure<Void, Domain.Error>(ex.Error);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }

                return Result.Success<Void, Domain.Error>(default);
            }
        }

        public override bool IsReadOnly() => this.AreAllOnErrorActionsReadOnly();

        /// <summary>
        /// Sets the http response of the http request action data.
        /// </summary>
        /// <param name="response">The response returned by the request.</param>
        public async Task ProcessResponse(HttpResponseMessage response, HttpRequestActionData actionData)
        {
            var headers = response.Headers
                .Select(x => new KeyValuePair<string, StringValues>(x.Key.ToLower(), x.Value.ToArray())).ToDictionary();
            if (response.Content?.Headers != null)
            {
                // merge in the top level content headers too, since C# separated them on us.
                var contentHeaders = response.Content?.Headers?
                    .Select(x => new KeyValuePair<string, StringValues>(x.Key.ToLower(), x.Value.ToArray())).ToDictionary();
                contentHeaders.ForEach(ch => headers.Add(ch.Key, ch.Value));
            }

            var contentValue = await ContentPart.FromHttpContent(response.Content!);
            actionData.HttpResponse = new Response(
                (int)response.StatusCode,
                response.ReasonPhrase,
                headers,
                contentValue?.ContentType,
                contentValue?.CharacterSet,
                contentValue?.Content);
        }

        /// <summary>
        /// Generate the flurl request with a certificate or none.
        /// </summary>
        private async Task<IFlurlRequest> GenerateFlurlRequest(Request request, IProviderContext providerContext)
        {
            IFlurlRequest? flurlRequest = null;

            if (request.ClientCertificateData != null)
            {
                X509Certificate2? certFromMemory = null;
                X509KeyStorageFlags flags = X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;
                try
                {
                    // test if has wrong password.
                    certFromMemory = new X509Certificate2(
                        request.ClientCertificateData.CertificateData,
                        request.ClientCertificateData.Password,
                        flags);
                }
                catch (CryptographicException e)
                {
                    var errorData = await providerContext.GetDebugContext();
                    errorData.Add("format", request.ClientCertificateData.CertificateFormat.ToLower());
                    Error? error = null;
                    if (e.Message.Contains("Cannot find the requested object."))
                    {
                        error = Errors.Automation.HttpRequest.CustomCertificateInvalidDataFormat(
                            request.ClientCertificateData.CertificateFormat,
                            errorData);
                    }

                    // It only has one error if incorrect password OR No password protection.
                    if (e.Message.Contains("The specified network password is not correct."))
                    {
                        error = Errors.Automation.HttpRequest.CustomCertificateIncorrectPassword(errorData);

                        // check if has no password instead, there is no way to know beforehand.
                        try
                        {
                            certFromMemory = new X509Certificate2(request.ClientCertificateData.CertificateData);

                            // the certificate doesnt have password if last line worked.
                            error = Errors.Automation.HttpRequest.CustomCertificateNoPasswordProtection(errorData);
                        }
                        catch
                        {
                        }
                    }

                    if (e.Message.Contains("The system cannot find the file specified."))
                    {
                        error = Errors.Automation.HttpRequest.CustomCertificateInvalidCertificate(errorData);
                    }

                    if (error != null)
                    {
                        throw new ErrorException(error);
                    }

                    throw e;
                }

                using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.MaxAllowed);
                    X509Certificate2Collection certCollection =
                       store.Certificates.Find(X509FindType.FindByThumbprint, certFromMemory.Thumbprint, false);
                    var certFromStore = certCollection.Any() ? certCollection[0] : null;
                    var flurlClient = new FlurlClient()
                        .Configure(settings =>
                        {
                            settings.HttpClientFactory = new X509HttpFactory(certFromStore ?? certFromMemory);
                        });
                    request.Headers.ForEach(header => flurlClient.Headers.Add(header.Key, header.Value));
                    flurlRequest = flurlClient.Request(request.Url);
                }
            }

            return (flurlRequest ?? request.Url.WithHeaders(request.Headers)).WithTimeout(this.requestTimeoutInMilliseconds);
        }

        private async Task<Domain.Error> ProcessException(Exception ex, IProviderContext providerContext, HttpRequestActionData actionData, ClientCertificateData? clientCertificateData)
        {
            HttpResponseMessage? errorResponse = default;
            if (ex is FlurlHttpException requestException)
            {
                errorResponse = requestException.Call?.HttpResponseMessage;
            }

            var request = actionData.HttpRequest;
            JObject errorData = await providerContext.GetDebugContext();
            var requestObject = JObject.FromObject(request);
            errorData.Add("Exception", ex?.Message + " " + ex?.InnerException?.Message + " " + ex?.InnerException?.InnerException?.Message);
            errorData.Add("requestUrl", request.Url);
            errorData.Add("requestMethod", request.HttpVerb.ToString());
            if (request.Headers == null)
            {
                string requestHeaders = string.Join(", ", request.Headers!.Select(header => $"{header.Key}: {header.Value}"));
                errorData.Add("requestHeaders", requestHeaders);
            }

            errorData.Add("requestContentType", request.ContentType);
            errorData.Add("requestCharacterSet", request.CharacterSet);

            if (request.Content is string stringContent)
            {
                requestObject["content"] = stringContent;
            }
            else if (request.Content is JToken jTokenContent)
            {
                requestObject["content"] = jTokenContent;
            }
            else if (request.Content is ContentPart)
            {
                requestObject["content"] = "[multipart content]";
            }
            else if (request.Content is byte[])
            {
                requestObject["content"] = "[binary content]";
            }
            else if (request.Content is null)
            {
                requestObject["content"] = null;
            }
            else
            {
                requestObject["content"] = $"[content was an unexpected type: {request.Content.GetType()}]";
            }

            Domain.Error errorDetails;
            if (errorResponse != default || ex is UriFormatException)
            {
                var responseContentString = await errorResponse!.Content.ReadAsStringAsync();
                errorData.Add("responseHttpStatusCode", string.Empty + (int)errorResponse.StatusCode);
                errorData.Add("responseContent", responseContentString);
                await this.ProcessResponse(errorResponse, actionData);
                errorDetails = Errors.Automation.HttpRequest.HttpResponseError(this.Alias, ex!.Message, errorResponse.StatusCode, errorData);
            }
            else
            {
                if (clientCertificateData != null &&
                    ex?.InnerException?.InnerException != null &&
                    ex.InnerException.InnerException.Message.ToLower().Contains("Could not create SSL/TLS secure channel.".ToLower()))
                {
                    errorDetails = Errors.Automation.HttpRequest.CustomCertificateInvalidCertificate(errorData);
                }
                else
                {
                    errorDetails = Errors.Automation.HttpRequest.HttpRequestError(this.Alias, ex!.Message, errorData);
                }
            }

            return errorDetails;
        }
    }
}
