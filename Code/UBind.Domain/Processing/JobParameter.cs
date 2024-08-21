// <copyright file="JobParameter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Processing
{
    using UBind.Domain;

    /// <summary>
    /// For specifying job parameters to add to Hangfire background jobs.
    /// </summary>
    /// <remarks>
    /// We primarily use these parameters to mark up jobs with additional information about what the job relates so our
    /// monitoring can distinguish production issues, and for viewing in the dashboard.
    /// </remarks>
    public class JobParameter
    {
        public const string TenantParameterName = "tenant";

        public const string OrganisationParameterName = "organisation";

        public const string ProductParameterName = "product";

        public const string EnvironmentParameterName = "environment";

        public const string IsAcknowledgedParameterName = "isAcknowledged";

        public JobParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; }

        public string Value { get; }

        public static JobParameter Tenant(string tenantAlias) =>
            new JobParameter(TenantParameterName, tenantAlias);

        public static JobParameter Organisation(string organisationAlias) =>
            new JobParameter(OrganisationParameterName, organisationAlias);

        public static JobParameter Product(string productAlias) =>
            new JobParameter(ProductParameterName, productAlias);

        public static JobParameter Environment(DeploymentEnvironment environment) =>
            new JobParameter(EnvironmentParameterName, environment.ToString());
    }
}
