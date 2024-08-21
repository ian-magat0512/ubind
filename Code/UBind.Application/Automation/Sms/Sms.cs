// <copyright file="Sms.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Sms
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Sms
    {
        public Sms(IEnumerable<string> to, string? from, string message)
        {
            this.To = to;
            this.Message = message;
            this.From = from;
        }

        [JsonProperty("to")]
        public IEnumerable<string> To { get; set; }

        [JsonProperty("from")]
        public string? From { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
