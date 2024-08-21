// <copyright file="FieldSerializationBinder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Serialization;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// When we write the type of the Field to json, we use a custom string. So we know which
    /// Type to instantiate during deserialization, we need a mapping. This binder does that for us.
    /// </summary>
    public class FieldSerializationBinder : DefaultSerializationBinder, IFieldSerializationBinder
    {
        private Dictionary<string, Type> nameToTypeMap = new Dictionary<string, Type>();
        private Dictionary<Type, string> typeToNameMap = new Dictionary<Type, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSerializationBinder"/> class.
        /// </summary>
        public FieldSerializationBinder()
        {
            IEnumerable<Type> types = typeof(FieldSerializationBinder).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Field)));
            foreach (Type type in types)
            {
                object[] attributes = type.GetCustomAttributes(typeof(JsonFieldTypeAttribute), false);
                foreach (var attribute in attributes)
                {
                    this.nameToTypeMap.Add((attribute as JsonFieldTypeAttribute).FieldType, type);
                    this.typeToNameMap.Add(type, (attribute as JsonFieldTypeAttribute).FieldType);
                }
            }
        }

        /// <inheritdoc/>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (!this.typeToNameMap.ContainsKey(serializedType))
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
                return;
            }

            var name = this.typeToNameMap[serializedType];
            assemblyName = null;
            typeName = name;
        }

        /// <inheritdoc/>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (this.nameToTypeMap.ContainsKey(typeName))
            {
                return this.nameToTypeMap[typeName];
            }

            return base.BindToType(assemblyName, typeName);
        }
    }
}
