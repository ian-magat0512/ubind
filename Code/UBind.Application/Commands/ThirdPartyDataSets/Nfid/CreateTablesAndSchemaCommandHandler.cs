// <copyright file="CreateTablesAndSchemaCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Nfid
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Handler for command to create tables and schema to be used by NFID.
    /// </summary>
    public class CreateTablesAndSchemaCommandHandler : ICommandHandler<CreateTablesAndSchemaCommand, Unit>
    {
        private readonly INfidRepository nfidRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTablesAndSchemaCommandHandler  "/> class.
        /// </summary>
        /// <param name="nfidRepository">The NFID repository.</param>
        public CreateTablesAndSchemaCommandHandler(
            INfidRepository nfidRepository)
        {
            this.nfidRepository = nfidRepository;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CreateTablesAndSchemaCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string currentSuffix = await this.nfidRepository.GetExistingTableIndex() ?? "00";
            var rollingNumber = new RollingNumber(99, currentSuffix, "D2");
            string nextIndex = rollingNumber.GetNext();
            await this.nfidRepository.CreateTablesByIndex(nextIndex);
            await this.nfidRepository.CreateIndexes(nextIndex);
            return await Task.FromResult(Unit.Value);
        }
    }
}
