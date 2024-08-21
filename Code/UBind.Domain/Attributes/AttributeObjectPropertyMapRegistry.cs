// <copyright file="AttributeObjectPropertyMapRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <inheritdoc/>
    public class AttributeObjectPropertyMapRegistry<TAttribute> : IAttributeObjectPropertyMapRegistry<TAttribute>
    {
        private Dictionary<Type, Dictionary<TAttribute, PropertyInfo>> typeMap =
            new Dictionary<Type, Dictionary<TAttribute, PropertyInfo>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeObjectPropertyMapRegistry{TAttribute}"/> class.
        /// </summary>
        public AttributeObjectPropertyMapRegistry()
        {
            IEnumerable<PropertyInfo> properties = typeof(AttributeObjectPropertyMapRegistry<TAttribute>).Assembly
                .GetTypes()
                .SelectMany(x => x.GetProperties())
                .Where(y => y.GetCustomAttributes().OfType<TAttribute>().Any());

            foreach (PropertyInfo property in properties)
            {
                this.RegisterPropertyInTypeMaps(property);
            }
        }

        /// <summary>
        /// Gets a map of the attribute to the property info it is set on, for each usage of the
        /// TAttribute on a given type.
        /// </summary>
        /// <param name="type">The type of object the attrubute is set on.</param>
        /// <returns>A dictionary which maps TAttribute to Properties.</returns>
        public Dictionary<TAttribute, PropertyInfo> GetAttributeToPropertyMap(Type type)
        {
            Dictionary<TAttribute, PropertyInfo> columnMap = null;
            if (!this.typeMap.TryGetValue(type, out columnMap))
            {
                throw new ErrorException(Errors.Product.WorkbookParseFailure(
                    $"When trying to parse a uBind workbook and create an object of type {type}, there was no "
                    + $"map found. Does {type} have any properties with the {typeof(TAttribute)}?"));
            }

            return columnMap;
        }

        private void RegisterPropertyInTypeMaps(PropertyInfo property)
        {
            List<Type> classesWithProperty = property.DeclaringType.Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(property.DeclaringType) && !type.IsAbstract).ToList();
            if (!property.DeclaringType.IsAbstract)
            {
                classesWithProperty.Add(property.DeclaringType);
            }

            foreach (Type type in classesWithProperty)
            {
                Dictionary<TAttribute, PropertyInfo> attributeMap = null;
                if (!this.typeMap.TryGetValue(type, out attributeMap))
                {
                    attributeMap = new Dictionary<TAttribute, PropertyInfo>();
                    this.typeMap.Add(type, attributeMap);
                }

                // There can be multiple instances of the WorkbookColumnNameAttribute on a single property:
                object[] attributes = property.GetCustomAttributes(typeof(TAttribute), false);
                foreach (var attribute in attributes)
                {
                    if (!attributeMap.ContainsKey((TAttribute)attribute))
                    {
                        attributeMap.Add((TAttribute)attribute, property);
                    }
                }
            }
        }
    }
}
