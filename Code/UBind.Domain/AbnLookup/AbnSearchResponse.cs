// <copyright file="AbnSearchResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.AbnLookup
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class is need to handle the response object model for ABN search.
    /// </summary>
    public class AbnSearchResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbnSearchResponse"/> class.
        /// </summary>
        /// <param name="abn">The ABN number.</param>
        /// <param name="abnStatus">The ABN status.</param>
        /// <param name="abnStatusEffectiveFrom">The ABN status effective from.</param>
        /// <param name="acn">The ACN.</param>
        /// <param name="addressDate">The address date.</param>
        /// <param name="addressPostcode">The address post code.</param>
        /// <param name="addressState">The address state.</param>
        /// <param name="businessNames">The list of business names.</param>
        /// <param name="entityName">The entity name.</param>
        /// <param name="entityTypeName">The entity type name.</param>
        /// <param name="entityTypeCode">The entity type code.</param>
        /// <param name="mainTradingName">The main trading name.</param>
        /// <param name="otherTradingNames">The list of other trading names.</param>
        /// <param name="gstRegistrationDate">The GST registration date.</param>
        public AbnSearchResponse(
            string abn,
            string abnStatus,
            DateTime abnStatusEffectiveFrom,
            string acn,
            DateTime addressDate,
            string addressPostcode,
            string addressState,
            List<string> businessNames,
            string entityName,
            string entityTypeName,
            string entityTypeCode,
            string mainTradingName,
            List<string> otherTradingNames,
            DateTime? gstRegistrationDate)
        {
            this.Abn = abn.Insert(2, " ").Insert(6, " ").Insert(10, " ");
            this.AbnStatus = abnStatus?.ToLower();
            this.AbnStatusEffectiveFrom = abnStatusEffectiveFrom.ToString("yyyy-MM-dd").ToNullIfWhitespace();
            this.Acn = acn.IsNullOrWhitespace() ? null : acn.Insert(3, " ").Insert(7, " ");
            this.AddressDate = addressDate.ToString("yyyy-MM-dd").ToNullIfWhitespace();
            this.AddressPostcode = int.TryParse(addressPostcode, out var postCode) ? postCode : (int?)null;
            this.AddressState = addressState.ToNullIfWhitespace();
            this.BusinessNames = businessNames;
            this.EntityName = entityName.ToNullIfWhitespace();
            this.EntityTypeCode = entityTypeCode.ToNullIfWhitespace();
            this.EntityTypeName = entityTypeName.ToNullIfWhitespace();
            this.MainTradingName = mainTradingName;
            this.OtherTradingNames = otherTradingNames;
            this.GstRegistrationDate =
                gstRegistrationDate.HasValue ? gstRegistrationDate.Value.ToString("yyyy-MM-dd") : null;
        }

        /// <summary>
        /// Gets the ABN registration number.
        /// </summary>
        public string Abn { get; private set; }

        /// <summary>
        /// Gets the ABN status whether active or cancelled.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AbnStatus { get; private set; }

        /// <summary>
        /// Gets ABN status effectivite from.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AbnStatusEffectiveFrom { get; private set; }

        /// <summary>
        /// Gets the ACN.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Acn { get; private set; }

        /// <summary>
        /// Gets the address date.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AddressDate { get; private set; }

        /// <summary>
        /// Gets the address post code.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? AddressPostcode { get; private set; }

        /// <summary>
        /// Gets the address state.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AddressState { get; private set; }

        /// <summary>
        /// Gets the business names.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> BusinessNames { get; private set; }

        /// <summary>
        /// Gets the entity name.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityName { get; private set; }

        /// <summary>
        /// Gets the entity type code.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityTypeCode { get; private set; }

        /// <summary>
        /// Gets the entity type name.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityTypeName { get; private set; }

        /// <summary>
        /// Gets the main trading name.
        /// </summary>
        public string MainTradingName { get; private set; }

        /// <summary>
        /// Gets the other trading names.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> OtherTradingNames { get; private set; }

        /// <summary>
        /// Gets the GST registration date.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GstRegistrationDate { get; private set; }
    }
}
