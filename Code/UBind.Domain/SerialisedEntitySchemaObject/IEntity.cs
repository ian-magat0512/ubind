// <copyright file="IEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;

    /// <summary>
    /// Interface for related entities.
    /// </summary>
    public interface IEntity : ISchemaObject
    {
        Type DomainEntityType { get; }

        bool SupportsAdditionalProperties { get; }

        Guid Id { get; set; }

        string EntityReference { get; set; }

        string EntityDescriptor { get; set; }

        string EntityEnvironment { get; set; }

        bool SupportsFileAttachment { get; set; }

        bool IsLoaded { get; }
    }
}
