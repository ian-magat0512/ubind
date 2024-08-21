// <copyright file="NumberPoolAddResultDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Collections.Generic;
    using UBind.Domain;

    /// <summary>
    /// Resource model for serving resource number load result.
    /// </summary>
    public class NumberPoolAddResultDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberPoolAddResultDto"/> class.
        /// </summary>
        /// <param name="addedNumbers">The reference number added.</param>
        /// <param name="duplicateNumbers">The duplicate reference numbers.</param>
        /// <param name="environment">The environment which numbers would be used for.</param>
        public NumberPoolAddResultDto(IEnumerable<string> addedNumbers, IEnumerable<string> duplicateNumbers, DeploymentEnvironment environment)
        {
            this.AddedNumbers = addedNumbers;
            this.DuplicateNumbers = duplicateNumbers;
            this.Environment = environment.ToString();
        }

        /// <summary>
        /// Gets the resource numbers added.
        /// </summary>
        public IEnumerable<string> AddedNumbers { get; }

        /// <summary>
        /// Gets the duplicate resource numbers.
        /// </summary>
        public IEnumerable<string> DuplicateNumbers { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public string Environment { get; }
    }
}
