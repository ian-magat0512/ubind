﻿// <copyright file="QuoteReadModelFilters.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using UBind.Domain.Enums;
    using UBind.Domain.Search;

    /// <summary>
    /// Represents filters to apply when querying quote read models.
    /// </summary>
    public class QuoteReadModelFilters : EntityListFilters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyReadModelFilters"/> class.
        /// </summary>
        public QuoteReadModelFilters()
        {
            this.SortBy = nameof(IQuoteSearchResultItemReadModel.LastModifiedTicksSinceEpoch);
            this.SortOrder = SortDirection.Descending;
        }
    }
}
