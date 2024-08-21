// <copyright file="PortalReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Portal
{
    using System.ComponentModel.DataAnnotations.Schema;
    using UBind.Domain.Exceptions;

    public class PortalReadModel : PortalReadModelSummary
    {
        static PortalReadModel()
        {
            SupportsAdditionalProperties = true;
        }

        public string StyleSheetUrl { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Styles { get; set; }

        public string? ProductionUrl { get; set; }

        public string? StagingUrl { get; set; }

        public string? DevelopmentUrl { get; set; }

        public string? GetEnvironmentUrlIfSet(DeploymentEnvironment environment)
        {
            switch (environment)
            {
                case DeploymentEnvironment.Production:
                    return this.ProductionUrl;
                case DeploymentEnvironment.Staging:
                    return this.StagingUrl;
                case DeploymentEnvironment.Development:
                    return this.DevelopmentUrl;
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(environment, typeof(DeploymentEnvironment)));
            }
        }
    }
}
