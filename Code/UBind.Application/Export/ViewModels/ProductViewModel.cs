// <copyright file="ProductViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// View model for presenting the tenant for razor templates.
    /// </summary>
    public class ProductViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductViewModel"/> class.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public ProductViewModel(JObject obj)
        {
            this.Title = obj["ProductTitle"]?.ToString();
            this.Description = "N/A";
        }

        /// <summary>
        /// Gets the Name or Title of the product.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the description of the product.
        /// </summary>
        public string Description { get; private set; }
    }
}
