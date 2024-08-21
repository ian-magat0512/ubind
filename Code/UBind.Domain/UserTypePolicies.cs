// <copyright file="UserTypePolicies.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Role Policies Helper.
    /// </summary>
    public class UserTypePolicies
    {
        /// <summary>
        /// All other roles except UbindAdmin.
        /// </summary>
        public const string AllExceptMaster = "AllExceptMaster";

        /// <summary>
        /// Either client admin or ubind admin role.
        /// </summary>
        public const string ClientOrMaster = "ClientOrMaster";

        /// <summary>
        /// Client admin role only. formerly ClientAdmin or Agent.
        /// </summary>
        public const string Client = "Client";

        /// <summary>
        /// Customer role only.
        /// </summary>
        public const string Customer = "Customer";

        /// <summary>
        /// Master role only. formerly ubindAdmin.
        /// </summary>
        public const string Master = "Master";

        /// <summary>
        /// All roles.
        /// </summary>
        public const string All = "All";
    }
}
