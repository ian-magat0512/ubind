// <copyright file="RolePolicyMapping.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Mapping
{
    using System.Collections.Generic;
    using UBind.Domain;

    /// <summary>
    /// Role Policy Mapping.
    /// </summary>
    public static class RolePolicyMapping
    {
        static RolePolicyMapping()
        {
#pragma warning disable SA1118 // Parameter must not span multiple lines

            Mapping = new Dictionary<string, List<string>>();

            Mapping.Add(
                UserTypePolicies.AllExceptMaster,
                value: new List<string>()
                {
                    UserTypePolicies.Customer,
                    UserTypePolicies.Client,
                });

            Mapping.Add(
             UserTypePolicies.All,
             value: new List<string>()
             {
                    UserTypePolicies.Customer,
                    UserTypePolicies.Client,
                    UserTypePolicies.Master,
             });

            Mapping.Add(
                UserTypePolicies.ClientOrMaster,
                value: new List<string>()
                {
                    UserTypePolicies.Client,
                    UserTypePolicies.Master,
                });

            Mapping.Add(
                UserTypePolicies.Client,
                value: new List<string>()
                {
                    UserTypePolicies.Client,
                });

            Mapping.Add(
                UserTypePolicies.Customer,
                value: new List<string>()
                {
                    UserTypePolicies.Customer,
                });

            Mapping.Add(
                UserTypePolicies.Master,
                value: new List<string>()
                {
                    UserTypePolicies.Master,
                });
        }

        /// <summary>
        /// Gets or sets role mapping.
        /// </summary>
        public static Dictionary<string, List<string>> Mapping { get; set; }
    }
}
