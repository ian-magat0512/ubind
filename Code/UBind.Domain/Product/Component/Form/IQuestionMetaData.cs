// <copyright file="IQuestionMetaData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Represents the Question Metadta.
    /// </summary>
    public interface IQuestionMetaData
    {
        /// <summary>
        /// Gets the questionkey.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets a value indicating whether Can change when approved.
        /// </summary>
        bool CanChangeWhenApproved { get; }

        /// <summary>
        /// Gets a value indicating whether to reset field for new quotes.
        /// </summary>
        bool ResetForNewQuotes { get; }

        /// <summary>
        /// Gets a value indicating whether to reset field for new renewal quotes.
        /// </summary>
        bool ResetForNewRenewalQuotes { get; }

        /// <summary>
        /// Gets a value indicating whether to reset field for new adjustment quotes.
        /// </summary>
        bool ResetForNewAdjustmentQuotes { get; }

        /// <summary>
        /// Gets a value indicating whether to reset field for new cancelation quotes.
        /// </summary>
        bool ResetForNewCancellationQuotes { get; }

        /// <summary>
        /// Gets a value indicating whether to reset field for new purchase quotes.
        /// </summary>
        bool ResetForNewPurchaseQuotes { get; }

        /// <summary>
        /// Gets a value indicating the data type of the question field.
        /// </summary>
        DataType DataType { get; }

        /// <summary>
        /// Gets the key of the parent question set.
        /// </summary>
        string ParentQuestionSetKey { get; }

        /// <summary>
        /// Gets the data type of the parent question set.
        /// </summary>
        DataType? ParentQuestionSetDataType { get; }

        /// <summary>
        /// Gets a value indicating whether a repeating field or not.
        /// </summary>
        bool IsWithinRepeatingQuestionSet { get; }

        /// <summary>
        /// Gets or sets the currency code if applicable.
        /// </summary>
        string CurrencyCode { get; set; }
    }
}
