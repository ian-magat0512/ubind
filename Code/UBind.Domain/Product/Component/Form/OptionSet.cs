// <copyright file="OptionSet.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using UBind.Domain.Extensions;
    using UBind.Domain.Validation;

    /// <summary>
    /// Represents a set of options.
    /// </summary>
    public class OptionSet : INamed
    {
        private string key;

        /// <summary>
        /// Gets or sets the name of the option set.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key for the option set.
        /// </summary>
        public string Key
        {
            get
            {
                if (this.key == null)
                {
                    return this.Name.ToCamelCase();
                }

                return this.key;
            }

            set
            {
                this.key = value;
            }
        }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        [Required]
        [ValidateItems]
        public List<Option> Options { get; set; } = new List<Option>();
    }
}
