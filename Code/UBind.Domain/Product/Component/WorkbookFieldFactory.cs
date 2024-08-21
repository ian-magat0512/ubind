// <copyright file="WorkbookFieldFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Factory for instantiating Fields from the type specified in the uBind workbook.
    /// This uses reflection so it's populated on first use and then cached in memory.
    /// </summary>
    public class WorkbookFieldFactory : IWorkbookFieldFactory
    {
        private Dictionary<string, Type> typeMap;
        private object mapLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookFieldFactory"/> class.
        /// </summary>
        public WorkbookFieldFactory()
        {
        }

        /// <inheritdoc/>
        public Field Create(string workbookFieldType)
        {
            lock (this.mapLock)
            {
                if (this.typeMap == null)
                {
                    this.PopulateTypeMap();
                }
            }

            if (this.typeMap.TryGetValue(workbookFieldType, out Type type))
            {
                return (Field)Activator.CreateInstance(type);
            }

            throw new ErrorException(
                Errors.Product.WorkbookParseFailure("When parsing a field in the workbook, the field type specified was "
                + $"\"{workbookFieldType}\", however that is not a valid or known field type."));
        }

        private void PopulateTypeMap()
        {
            this.typeMap = new Dictionary<string, Type>();
            IEnumerable<Type> types = typeof(WorkbookFieldFactory).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Field)));
            foreach (Type type in types)
            {
                object[] attributes = type.GetCustomAttributes(typeof(WorkbookFieldTypeAttribute), false);
                foreach (var attribute in attributes)
                {
                    this.typeMap.Add((attribute as WorkbookFieldTypeAttribute).FieldType, type);
                }
            }
        }
    }
}
