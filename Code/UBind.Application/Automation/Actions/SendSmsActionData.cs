// <copyright file="SendSmsActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    public class SendSmsActionData : ActionData
    {
        public SendSmsActionData(string name, string alias, IClock clock)
            : base(name, alias, ActionType.SendSmsAction, clock)
        {
        }

        [JsonConstructor]
        public SendSmsActionData()
            : base(ActionType.SendSmsAction)
        {
        }

        [JsonProperty(PropertyName = "sms", NullValueHandling = NullValueHandling.Ignore)]
        public Sms.Sms Sms { get; set; }

        [JsonProperty(PropertyName = "tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tags { get; set; }

        [JsonProperty(PropertyName = "relationships", NullValueHandling = NullValueHandling.Ignore)]
        public List<UBind.Domain.SerialisedEntitySchemaObject.Relationship> Relationships { get; set; }
    }
}
