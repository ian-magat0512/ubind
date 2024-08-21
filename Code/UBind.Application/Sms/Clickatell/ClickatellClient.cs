// <copyright file="ClickatellClient.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Sms.Clickatell
{
    using System.Net;
    using System.Threading.Tasks;
    using Flurl.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Sms;
    using UBind.Domain.Extensions;

    public class ClickatellClient : ISmsClient
    {
        private readonly ISmsConfiguration smsConfiguration;

        public ClickatellClient(ISmsConfiguration smsConfiguration)
        {
            this.smsConfiguration = smsConfiguration;
        }

        public async Task<SmsResponse> SendSms(Sms sms)
        {
            try
            {
                var serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };
                var request = JObject.FromObject(new ClickatellRequest(sms), serializer);
                var clickatellResponse = await this.smsConfiguration.Url
                    .WithHeader("Authorization", this.smsConfiguration.ApiKey)
                    .PostJsonAsync(request)
                    .ReceiveJson<ClickatellResponse>();
                var response = new SmsResponse(SmsResponseType.Success, sms);
                return response;
            }
            catch (FlurlHttpException ex)
            {
                var request = ex.Call.Request;
                var response = ex.Call.Response;
                var rawErrorResponse = await response.ResponseMessage.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ClickatellResponse>(rawErrorResponse);
                string? errorMessage = null;
                JObject errorData = new JObject();
                var smsResponseType = SmsResponseType.Error;
                errorData.Add("requestUrl", request.Url.ToString());
                errorData.Add("requestHeaders", request.Headers.ToString()
                    .Replace(this.smsConfiguration.ApiKey, this.smsConfiguration.ApiKey.ToMaskedName()));
                errorData.Add("requestError", ex.Message);
                errorData.Add("responseStatusCode", (int)response.StatusCode);
                errorData.Add("responseContent", rawErrorResponse);

                // The error message is not always in the same place in the response, so we need to check a few places.
                // Usually, it's in the Messages array property, but sometimes it's in the Error property.
                if (errorResponse?.Messages != null)
                {
                    errorMessage = string.Join(", ", errorResponse.Messages.Select(a => a.Error.Description));
                }
                else if (errorResponse?.Error.Description != null)
                {
                    errorMessage = errorResponse.Error.Description;
                }

                // If we can't find one, we'll generate one based on the status code.
                switch (ex.Call.HttpResponseMessage.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        errorMessage = errorMessage ?? "Unauthorized access to resource.";
                        break;
                    case HttpStatusCode.NotFound:
                        errorMessage = errorMessage ?? "Request was sent to a resource that does not exist.";
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        errorMessage = errorMessage ?? "Service unavailable.";
                        break;
                    default:
                        errorMessage = errorMessage ?? "There was a general failure communicating with the Clickatell service.";
                        smsResponseType = SmsResponseType.Failed;
                        break;
                }

                errorData.Add("errorDescription", errorMessage);
                return new SmsResponse(smsResponseType, sms, errorData, errorMessage);
            }
        }
    }
}
