// <copyright file="IFormDataPrettifier.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Product;

    /// <summary>
    /// Contract for a class which reads meta data about fields and formats the field values appropriately.
    /// </summary>
    public interface IFormDataPrettifier
    {
        /// <summary>
        /// Get prettified form data as a json string, from a json string.
        /// </summary>
        /// <param name="questionsMetaData">the form's questions metadata.</param>
        /// <param name="jsonFormData">The raw json.</param>
        /// <param name="entityId">For debugging purposes, the entity id the form data is for.</param>
        /// <param name="entityType">For debugging purposes, the entity type the form data is for, e.g. Quote or Claim.</param>
        /// <param name="productContext">For debugging purposes, the product context.</param>
        /// <returns>return the updated form data.</returns>
        JObject GetPrettifiedFormData(
            IEnumerable<IQuestionMetaData> questionsMetaData,
            string jsonFormData,
            Guid entityId,
            string entityType,
            IProductContext productContext);

        /// <summary>
        /// Get prettified Calculation Result as a json string, from a json string.
        /// </summary>
        /// <param name="questionsMetaData">the form's questions metadata.</param>
        /// <param name="jsonCalculationResult">The raw Calculation Result json.</param>
        /// <param name="entityId">For debugging purposes, the entity id the form data is for.</param>
        /// <param name="entityType">For debugging purposes, the entity type the form data is for, e.g. Quote or Claim.</param>
        /// <param name="productContext">For debugging purposes, the product context.</param>
        /// <returns>return the updated Calculation Result json string.</returns>
        JObject GetPrettifiedCalculationResult(
            IEnumerable<IQuestionMetaData> questionsMetaData,
            string jsonCalculationResult,
            Guid entityId,
            string entityType,
            IProductContext productContext);

        /// <summary>
        /// Prettifies form data by modifying the passed JObject and changing it's values.
        /// </summary>
        /// <param name="questionsMetaData">the form's questions metadata.</param>
        /// <param name="jObject">The json object to be updated.</param>
        /// <param name="entityId">For debugging purposes, the entity id the form data is for.</param>
        /// <param name="entityType">For debugging purposes, the entity type the form data is for, e.g. Quote or Claim.</param>
        /// <param name="productContext">For debugging purposes, the product context.</param>
        void Prettify(
            IEnumerable<IQuestionMetaData> questionsMetaData,
            JObject jObject,
            Guid entityId,
            string entityType,
            IProductContext productContext);
    }
}
