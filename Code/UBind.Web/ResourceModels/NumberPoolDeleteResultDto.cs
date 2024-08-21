// <copyright file="NumberPoolDeleteResultDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Collections.Generic;
    using UBind.Domain;

    /// <summary>
    /// Resource model for serving the number delete result for a pool of numbers.
    /// </summary>
    public class NumberPoolDeleteResultDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberPoolDeleteResultDto"/> class.
        /// </summary>
        /// <param name="deletedNumbers">The numbers deleted.</param>
        /// <param name="deploymentEnvironment">The deployment environment.</param>
        public NumberPoolDeleteResultDto(List<string> deletedNumbers, DeploymentEnvironment deploymentEnvironment)
        {
            this.DeletedNumbers = deletedNumbers;
            this.DeploymentEnvironment = deploymentEnvironment;
        }

        /// <summary>
        /// Gets the resource number deleted.
        /// </summary>
        public List<string> DeletedNumbers { get; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        public DeploymentEnvironment DeploymentEnvironment { get; }
    }
}
