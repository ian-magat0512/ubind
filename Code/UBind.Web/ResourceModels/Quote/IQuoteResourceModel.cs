﻿// <copyright file="IQuoteResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;

    /// <summary>
    /// Interface for accessing a request body's application ID.
    /// </summary>
    public interface IQuoteResourceModel
    {
        /// <summary>
        /// Gets the ID of the application the request relates to.
        /// </summary>
        Guid QuoteId { get; }

        Guid? ProductReleaseId { get; }
    }
}
