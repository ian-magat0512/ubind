// <copyright file="SmsResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Sms
{
    using Newtonsoft.Json.Linq;

    public class SmsResponse
    {
        public SmsResponse(SmsResponseType smsResponseType, Sms sms)
        {
            this.ResponseType = smsResponseType;
            this.Details = sms;
        }

        public SmsResponse(SmsResponseType smsResponseType, Sms sms, JObject errorData, string? errorMessage = null)
            : this(smsResponseType, sms)
        {
            this.ErrorData = errorData;
            this.ErrorMessage = errorMessage;
        }

        public JObject ErrorData { get; }

        public Sms Details { get; }

        public SmsResponseType ResponseType { get; }

        public string? ErrorMessage { get; }
    }
}
