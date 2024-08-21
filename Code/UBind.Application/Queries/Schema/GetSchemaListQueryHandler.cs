// <copyright file="GetSchemaListQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Schema
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Configuration;
    using UBind.Domain.Patterns.Cqrs;

    public class GetSchemaListQueryHandler : IQueryHandler<GetSchemaListQuery, List<string>>
    {
        private readonly ISchemaConfiguration schemaConfiguration;

        public GetSchemaListQueryHandler(ISchemaConfiguration schemaConfiguration)
        {
            this.schemaConfiguration = schemaConfiguration;
        }

        public Task<List<string>> Handle(GetSchemaListQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(this.schemaConfiguration.AllowedFileNames.ToList());
        }
    }
}
