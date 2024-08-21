// <copyright file="RelationshipRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Dapper;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class RelationshipRepository : IRelationshipRepository
    {
        private readonly IConnectionConfiguration connection;
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public RelationshipRepository(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection)
        {
            Contract.Assert(dbContext != null);
            this.dbContext = dbContext;
            this.connection = connection;
        }

        public bool Exists(Guid tenantId, Guid relationshipId)
        {
            return this.dbContext.Relationships.SingleOrDefault(r => r.TenantId == tenantId && r.Id == relationshipId) != default;
        }

        public void Insert(Relationship relationship)
        {
            this.dbContext.Relationships.Add(relationship);
        }

        public void AddRange(IEnumerable<Relationship> relationships)
        {
            this.dbContext.Relationships.AddRange(relationships);
        }

        /// <inheritdoc/>
        public int UpdateTopWithoutCreatedTicksEpochValue(int batch)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@count", batch);

            string sql = @"UPDATE TOP (@count) [Relationships]
                            SET [CreatedTicksSinceEpoch] = [CreationTimeInTicksSinceEpoch]
                            WHERE [CreatedTicksSinceEpoch] <> [CreationTimeInTicksSinceEpoch]";

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var rowsAffected = connection.Execute(sql, parameters, null, 0, System.Data.CommandType.Text);
                return rowsAffected;
            }
        }
    }
}
