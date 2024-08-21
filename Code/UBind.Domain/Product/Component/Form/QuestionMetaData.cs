// <copyright file="QuestionMetaData.cs" company="uBind">
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
    public class QuestionMetaData : IQuestionMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionMetaData"/> class.
        /// </summary>
        /// <param name="key">The Question key.</param>
        /// <param name="canChangeWhenApproved">indicator whether Can change when approved.</param>
        /// <param name="resetForNewQuotes">indicator whether to reset the specified field to default.</param>
        /// <param name="resetForNewRenewalQuotes">indicator whether to reset of new quotes the specified field to default.</param>
        /// <param name="resetForNewAdjustmentQuotes">indicator whether to reset new adjustment quotes the specified field to default.</param>
        /// <param name="resetForNewCancellationQuotes">indicator whether to reset new cancelation quotes the specified field to default.</param>
        /// <param name="resetForNewPurchaseQuotes">indicator whether to reset new purchase quotes the specified field to default.</param>
        /// <param name="dataType">The field's data type.</param>
        /// <param name="parentQuestionSetKey">The parent question set's key.</param>
        /// <param name="parentQuestionSetDataType">The parent question set's data type.</param>
        /// <param name="isWithinRepeatingQuestionSet">Indicator whether the field is within a repeating question set or not.</param>
        /// <param name="currencyCode">The currency code, or pass null if it's not relevant or it's unknown.</param>
        public QuestionMetaData(
            string key,
            bool canChangeWhenApproved,
            bool resetForNewQuotes,
            bool resetForNewRenewalQuotes,
            bool resetForNewAdjustmentQuotes,
            bool resetForNewCancellationQuotes,
            bool resetForNewPurchaseQuotes,
            DataType dataType,
            string parentQuestionSetKey,
            DataType? parentQuestionSetDataType,
            bool isWithinRepeatingQuestionSet,
            string currencyCode)
        {
            this.Key = key;
            this.CanChangeWhenApproved = canChangeWhenApproved;
            this.ResetForNewQuotes = resetForNewQuotes;
            this.ResetForNewRenewalQuotes = resetForNewRenewalQuotes;
            this.ResetForNewAdjustmentQuotes = resetForNewAdjustmentQuotes;
            this.ResetForNewCancellationQuotes = resetForNewCancellationQuotes;
            this.ResetForNewPurchaseQuotes = resetForNewPurchaseQuotes;
            this.DataType = dataType;
            this.IsWithinRepeatingQuestionSet = isWithinRepeatingQuestionSet;
            this.ParentQuestionSetKey = parentQuestionSetKey;
            this.CurrencyCode = currencyCode;
            this.ParentQuestionSetDataType = parentQuestionSetDataType;
        }

        /// <inheritdoc/>
        public string Key { get; }

        /// <inheritdoc/>
        public bool CanChangeWhenApproved { get; }

        /// <inheritdoc/>
        public bool ResetForNewQuotes { get; }

        /// <inheritdoc/>
        public bool ResetForNewRenewalQuotes { get; }

        /// <inheritdoc/>
        public bool ResetForNewAdjustmentQuotes { get; }

        /// <inheritdoc/>
        public bool ResetForNewCancellationQuotes { get; }

        /// <inheritdoc/>
        public bool ResetForNewPurchaseQuotes { get; }

        /// <inheritdoc/>
        public DataType DataType { get; }

        /// <inheritdoc/>
        public string ParentQuestionSetKey { get; }

        /// <inheritdoc/>
        public DataType? ParentQuestionSetDataType { get; }

        /// <inheritdoc/>
        public bool IsWithinRepeatingQuestionSet { get; }

        /// <inheritdoc/>
        public string CurrencyCode { get; set; }
    }
}
