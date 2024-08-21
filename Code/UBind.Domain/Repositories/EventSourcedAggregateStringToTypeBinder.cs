// <copyright file="EventSourcedAggregateStringToTypeBinder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Serialization;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Aggregates.Quote.Entities;
    using UBind.Domain.Entities;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    /// <summary>
    /// Allows us to control class loading and mandate what class to load.
    /// </summary>
    public class EventSourcedAggregateStringToTypeBinder : ISerializationBinder
    {
        private readonly Dictionary<string, Type> typeMappings =
            new Dictionary<string, Type>
            {
                { "UBind.Domain.Entities.ClaimFileAttachments", typeof(ClaimFileAttachment) },
                { "UBind.Domain.Aggregates.Quote.Entities.PolicyTransaction", typeof(PolicyTransactionOld) },
                {
                    "System.Collections.Generic.List`1[[UBind.Domain.Aggregates.Quote.Entities.PolicyTransaction, UBind.Domain]]",
                    typeof(List<PolicyTransactionOld>)
                },
                {
                    "UBind.Domain.Aggregates.Quote.QuoteAggregate+QuoteOrganisationMigratedEvent",
                    typeof(QuoteTransferredToAnotherOrganisationEvent)
                },
                {
                    "UBind.Domain.Aggregates.Quote.QuoteAggregate+QuoteExpiryTimestampSetInBulkEvent",
                    typeof(QuoteExpiryTimestampSetEvent)
                },
                {
                    "System.Linq.OrderedEnumerable`2[[UBind.Domain.Aggregates.Person.Fields.EmailAddressField, UBind.Domain],[System.Int32, System.Private.CoreLib]]",
                    typeof(List<EmailAddressField>)
                },
                {
                    "System.Linq.OrderedEnumerable`2[[UBind.Domain.Aggregates.Person.Fields.StreetAddressField, UBind.Domain],[System.Int32, System.Private.CoreLib]]",
                    typeof(List<StreetAddressField>)
                },
                {
                    "System.Linq.OrderedEnumerable`2[[UBind.Domain.Aggregates.Person.Fields.PhoneNumberField, UBind.Domain],[System.Int32, System.Private.CoreLib]]",
                    typeof(List<PhoneNumberField>)
                },
                {
                    "System.Linq.OrderedEnumerable`2[[UBind.Domain.Aggregates.Person.Fields.MessengerIdField, UBind.Domain],[System.Int32, System.Private.CoreLib]]",
                    typeof(List<MessengerIdField>)
                },
                {
                    "System.Linq.OrderedEnumerable`2[[UBind.Domain.Aggregates.Person.Fields.WebsiteAddressField, UBind.Domain],[System.Int32, System.Private.CoreLib]]",
                    typeof(List<WebsiteAddressField>)
                },
                {
                    "System.Linq.OrderedEnumerable`2[[UBind.Domain.Aggregates.Person.Fields.SocialMediaIdField, UBind.Domain],[System.Int32, System.Private.CoreLib]]",
                    typeof(List<SocialMediaIdField>)
                },
                {
                    "System.Linq.Enumerable+SelectIListIterator`2[[UBind.Domain.ReadModel.Person.Fields.StreetAddressReadModel, UBind.Domain],[UBind.Domain.Aggregates.Person.Fields.StreetAddressField, UBind.Domain]]",
                    typeof(List<StreetAddressField>)
                },
            };

        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        /// <returns>The type of the object the formatter creates a new instance of.</returns>
        public Type BindToType(string assemblyName, string typeName)
        {
            if (this.typeMappings.TryGetValue(typeName, out Type mappedType))
            {
                return mappedType;
            }

            string fullTypeName = typeName;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                fullTypeName = $"{typeName}, {assemblyName}";
            }

            return Type.GetType(fullTypeName);
        }

        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = serializedType.Assembly.GetName().Name;
            typeName = serializedType.FullName;
        }
    }
}
