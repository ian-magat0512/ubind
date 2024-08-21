// <copyright file="CustomerMapping.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using Newtonsoft.Json;

    /// <summary>
    /// Container that represents the mapped customer properties.
    /// </summary>
    public class CustomerMapping
    {
        /// <summary>
        /// Gets the default customer mapping to be used.
        /// </summary>
        public static CustomerMapping Default
        {
            get
            {
                var defaultMapping = new CustomerMapping
                {
                    FullName = "FullName",
                    PreferredName = "PreferredName",
                    NamePrefix = "NamePrefix",
                    FirstName = "FirstName",
                    MiddleNames = "MiddleNames",
                    LastName = "LastName",
                    NameSuffix = "NameSuffix",
                    Company = "Company",
                    Title = "Title",
                    Email = "Email",
                    AlternativeEmail = "AlternativeEmail",
                    MobilePhone = "MobilePhone",
                    HomePhone = "HomePhone",
                    WorkPhone = "WorkPhone",
                };
                return defaultMapping;
            }
        }

        /// <summary>
        /// Gets the name property of the customer object.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the preferred name property of the customer object.
        /// </summary>
        [JsonProperty]
        public string PreferredName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the email property of the customer object.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Email { get; private set; }

        /// <summary>
        /// Gets the alternative email property of the customer object.
        /// </summary>
        [JsonProperty]
        public string AlternativeEmail { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the mobile phone property of the customer object.
        /// </summary>
        [JsonProperty]
        public string MobilePhone { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the home phone property of the customer object.
        /// </summary>
        [JsonProperty]
        public string HomePhone { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the work phone property of the customer object.
        /// </summary>
        [JsonProperty]
        public string WorkPhone { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the name prefix property of the customer object.
        /// </summary>
        [JsonProperty]
        public string NamePrefix { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the first name property of the customer object.
        /// </summary>
        [JsonProperty]
        public string FirstName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the first name property of the customer object.
        /// </summary>
        [JsonProperty]
        public string MiddleNames { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the middle names property of the customer object.
        /// </summary>
        [JsonProperty]
        public string LastName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the name suffix property of the customer object.
        /// </summary>
        [JsonProperty]
        public string NameSuffix { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the company property of the customer object.
        /// </summary>
        [JsonProperty]
        public string Company { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the title property of the customer object.
        /// </summary>
        [JsonProperty]
        public string Title { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the tenant ID property of the customer object.
        /// </summary>
        [JsonProperty]
        public string TenantId { get; private set; } = string.Empty;
    }
}
