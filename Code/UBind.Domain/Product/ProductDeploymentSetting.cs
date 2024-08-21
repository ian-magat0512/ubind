// <copyright file="ProductDeploymentSetting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System.Collections.Generic;

    /// <summary>
    /// Deployment Settings.
    /// </summary>
    public class ProductDeploymentSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDeploymentSetting"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        public ProductDeploymentSetting()
        {
            // add default values.
            this.Development = new List<string>();
            this.Staging = new List<string>();
            this.Production = new List<string>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether check if development is active.
        /// </summary>
        public bool DevelopmentIsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether check if staging is active.
        /// </summary>
        public bool StagingIsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether check if production is active.
        /// </summary>
        public bool ProductionIsActive { get; set; }

        /// <summary>
        /// Gets or sets the deployment settings.
        /// </summary>
        public List<string> Development { get; set; }

        /// <summary>
        /// Gets or sets the staging settings.
        /// </summary>
        public List<string> Staging { get; set; }

        /// <summary>
        /// Gets or sets the production settings.
        /// </summary>
        public List<string> Production { get; set; }
    }
}
