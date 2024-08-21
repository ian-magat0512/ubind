// <copyright file="NascentDeletionView.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Reduction
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// View model for nascent deletion view.
    /// </summary>
    public class NascentDeletionView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NascentDeletionView"/> class.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="propertyToReturn">The property to return.</param>
        /// <param name="data">The data of the query to search.</param>
        public NascentDeletionView(string tableName, string propertyToReturn, IEnumerable<Guid> data)
        {
            this.TableName = tableName;
            this.PropertyToReturn = propertyToReturn;
            this.Data = data;
        }

        /// <summary>
        /// Gets the data of query data to execute.
        /// </summary>
        public IEnumerable<Guid> Data { get; }

        /// <summary>
        /// Gets the table property to return.
        /// </summary>
        public string PropertyToReturn { get; }

        /// <summary>
        /// Gets the table name to return.
        /// </summary>
        public string TableName { get; }
    }
}
