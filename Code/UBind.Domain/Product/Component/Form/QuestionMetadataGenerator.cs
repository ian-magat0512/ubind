// <copyright file="QuestionMetadataGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Generates question meta data from a Form instance.
    /// </summary>
    public static class QuestionMetadataGenerator
    {
        /// <summary>
        /// Generates question meta data from a Form instance.
        /// </summary>
        /// <param name="form">The form instance.</param>
        /// <returns>A list of question metadata.</returns>
        public static IReadOnlyList<IQuestionMetaData> Generate(Form form)
        {
            string defaultCurrencyCode = form.DefaultCurrencyCode ?? PriceBreakdown.DefaultCurrencyCode;
            Dictionary<string, DataType?> questionSetKeyToDataTypeMap = new Dictionary<string, DataType?>();
            List<IQuestionMetaData> questionSetsMetadata = form.QuestionSets
                .SelectMany(questionSet => questionSet.Fields, (questionSet, field) => field)
                .Where(field => field is IDataStoringField)
                .Select(field =>
                {
                    if (field.DataType == DataType.Repeating)
                    {
                        questionSetKeyToDataTypeMap.Add(field.Key, field.DataType);
                    }

                    return new QuestionMetaData(
                        field.Key,
                        ((IDataStoringField)field).CanChangeWhenApproved ?? false,
                        ((IDataStoringField)field).ResetForNewQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewRenewalQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewAdjustmentQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewCancellationQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewPurchaseQuotes ?? false,
                        field.DataType,
                        field.QuestionSet.Key,
                        null,
                        false,
                        field.DataType == DataType.Currency ? field.CurrencyCode ?? defaultCurrencyCode : null);
                })
                .ToList<IQuestionMetaData>();

            List<IQuestionMetaData> repeatingQuestionSetsMetadata = form.RepeatingQuestionSets
                .SelectMany(questionSet => questionSet.Fields, (questionSet, field) => field)
                .Where(field => field is IDataStoringField)
                .Select(field =>
                {
                    questionSetKeyToDataTypeMap.TryGetValue(field.QuestionSet.Key, out DataType? parentDataType);
                    return new QuestionMetaData(
                        field.Key,
                        ((IDataStoringField)field).CanChangeWhenApproved ?? false,
                        ((IDataStoringField)field).ResetForNewQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewRenewalQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewAdjustmentQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewCancellationQuotes ?? false,
                        ((IDataStoringField)field).ResetForNewPurchaseQuotes ?? false,
                        field.DataType,
                        field.QuestionSet.Key,
                        parentDataType,
                        true,
                        field.DataType == DataType.Currency ? field.CurrencyCode ?? defaultCurrencyCode : null);
                })
                .ToList<IQuestionMetaData>();

            questionSetsMetadata.AddRange(repeatingQuestionSetsMetadata);
            return questionSetsMetadata;
        }
    }
}
