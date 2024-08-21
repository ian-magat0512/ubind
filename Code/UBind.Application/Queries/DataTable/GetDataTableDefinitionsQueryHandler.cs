// <copyright file="GetDataTableDefinitionsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.DataTable
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Dto;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Query handler for getting data table definition collection.
    /// </summary>
    public class GetDataTableDefinitionsQueryHandler : IQueryHandler<GetDataTableDefinitionsQuery, List<DataTableDefinitionDto>>
    {
        private readonly IDataTableDefinitionRepository dataTableRepository;

        public GetDataTableDefinitionsQueryHandler(IDataTableDefinitionRepository dataTableRepository)
        {
            this.dataTableRepository = dataTableRepository;
        }

        public Task<List<DataTableDefinitionDto>> Handle(GetDataTableDefinitionsQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataTableDefinitions = this.dataTableRepository.GetDataTableDefinitionsQuery(request.Filters).ToList();
            return Task.FromResult(dataTableDefinitions.Select(dataTableDefinition => new DataTableDefinitionDto(dataTableDefinition)).ToList());
        }
    }
}
