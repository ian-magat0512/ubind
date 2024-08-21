// <copyright file="PrimitivePropertyConfigurationExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.ModelConfiguration.Configuration;

    /// <summary>
    /// Extension methods for configuring entityy properties.
    /// </summary>
    internal static class PrimitivePropertyConfigurationExtensions
    {
        /// <summary>
        /// For applying multi-column unique indexes.
        /// </summary>
        /// <param name="property">The property to apply the unique index to.</param>
        /// <param name="indexName">THe index name.</param>
        /// <param name="columnOrder">The column order.</param>
        /// <returns>The instance of PrimitivePropertyConfiguration the method was called on, to enable fluent syntax.</returns>
        /// <remarks>
        /// Taken from https://stackoverflow.com/a/25779348/177018
        /// .</remarks>
        public static PrimitivePropertyConfiguration HasUniqueIndexAnnotation(
            this PrimitivePropertyConfiguration property,
            string indexName,
            int columnOrder)
        {
            var indexAttribute = new IndexAttribute(indexName, columnOrder) { IsUnique = true };
            var indexAnnotation = new IndexAnnotation(indexAttribute);
            return property.HasColumnAnnotation(IndexAnnotation.AnnotationName, indexAnnotation);
        }
    }
}
