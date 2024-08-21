// <copyright file="IRelationshipRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.ReadWriteModel;

    public interface IRelationshipRepository
    {
        bool Exists(Guid tenantId, Guid relationshipId);

        void Insert(Relationship relationship);

        void AddRange(IEnumerable<Relationship> relationships);

        /// <summary>
        /// Updates top X records without created ticks epoch.
        /// Note: this is specifically used for migration associated with renaming date fields.
        /// </summary>
        /// <param name="batch">how many to update.</param>
        /// <returns>Rows affected.</returns>
        int UpdateTopWithoutCreatedTicksEpochValue(int batch);
    }
}
