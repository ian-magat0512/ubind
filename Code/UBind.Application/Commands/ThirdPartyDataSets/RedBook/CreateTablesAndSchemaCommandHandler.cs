// <copyright file="CreateTablesAndSchemaCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.RedBook
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Represents the handler to create Gnaf tables and schema.
    /// </summary>
    public class CreateTablesAndSchemaCommandHandler : ICommandHandler<CreateTablesAndSchemaCommand, Unit>
    {
        private readonly IRedBookRepository redBookRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTablesAndSchemaCommandHandler"/> class.
        /// </summary>
        /// <param name="redBookRepository">The RedBook repository.</param>
        public CreateTablesAndSchemaCommandHandler(IRedBookRepository redBookRepository)
        {
            this.redBookRepository = redBookRepository;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CreateTablesAndSchemaCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentSuffix = await this.redBookRepository.GetExistingTableIndex() ?? "00";
            var rollingNumber = new RollingNumber(99, currentSuffix, "D2");
            string nextIndex = rollingNumber.GetNext();
            await this.redBookRepository.CreateTablesAndSchema(nextIndex);
            await this.redBookRepository.CreateForeignKeysAndIndexes(nextIndex);
            return await Task.FromResult(Unit.Value);
        }
    }
}