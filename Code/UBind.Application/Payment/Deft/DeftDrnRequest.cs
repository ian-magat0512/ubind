// <copyright file="DeftDrnRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using Newtonsoft.Json;

    /// <summary>
    /// A DEFT DRN needs to be generated before a payment can be processed.
    /// </summary>
    public class DeftDrnRequest
    {
        public DeftDrnRequest(string billerCode, string customerReferenceNumber)
        {
            this.BillerCode = ulong.Parse(billerCode);
            this.CustomerReferenceNumber = ulong.Parse(customerReferenceNumber);
        }

        public DeftDrnRequest(ulong billerCode, ulong customerReferenceNumber)
        {
            this.BillerCode = billerCode;
            this.CustomerReferenceNumber = customerReferenceNumber;
        }

        [JsonProperty("dbc")]
        public ulong BillerCode { get; }

        [JsonProperty("crn")]
        public ulong CustomerReferenceNumber { get; }
    }
}
