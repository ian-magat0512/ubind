// <copyright file="GetDataTableDefinitionByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.DataTable
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Query handler for getting a data table definition by ID.
    /// </summary>
    public class GetDataTableDefinitionByIdQueryHandler : IQueryHandler<GetDataTableDefinitionByIdQuery, DataTableDefinitionDto>
    {
        private readonly IDataTableDefinitionRepository dataTableDefinitionRepository;

        public GetDataTableDefinitionByIdQueryHandler(IDataTableDefinitionRepository dataTableDefinitionRepository)
        {
            this.dataTableDefinitionRepository = dataTableDefinitionRepository;
        }

        public Task<DataTableDefinitionDto> Handle(GetDataTableDefinitionByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataTableDefinition = this.dataTableDefinitionRepository.GetDataTableDefinitionById(request.TenantId, request.DataTableDefinitionId);
            if (dataTableDefinition == null)
            {
                throw new ErrorException(Domain.Errors.DataTableDefinition.NotFound(request.DataTableDefinitionId));
            }

            return Task.FromResult(new DataTableDefinitionDto(dataTableDefinition));
        }
    }
}
