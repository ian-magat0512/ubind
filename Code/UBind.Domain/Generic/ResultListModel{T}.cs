// <copyright file="ResultListModel{T}.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Generic
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// standardized result model for list of returns.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    public class ResultListModel<T> : ResultModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultListModel{T}"/> class.
        /// </summary>
        public ResultListModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultListModel{T}"/> class.
        /// </summary>
        /// <param name="items">the items.</param>
        public ResultListModel(IQueryable<T> items)
        {
            this.Items = items;
            this.Total = this.Items.Count();
            this.Successful = true;
            this.Message = "Successful";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultListModel{T}"/> class.
        /// </summary>
        /// <param name="items">the items.</param>
        public ResultListModel(List<T> items)
        {
            this.Items = items.AsQueryable();
            this.Total = this.Items.Count();
            this.Successful = true;
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public IQueryable<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public int Total { get; set; }
    }
}
