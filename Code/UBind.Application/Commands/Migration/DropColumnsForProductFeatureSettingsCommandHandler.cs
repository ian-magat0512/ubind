// <copyright file="DropColumnsForProductFeatureSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class DropColumnsForProductFeatureSettingsCommandHandler
        : ICommandHandler<DropColumnsForProductFeatureSettingsCommand, Unit>
    {
        private readonly IUBindDbContext dbContext;

        public DropColumnsForProductFeatureSettingsCommandHandler(
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<Unit> Handle(
            DropColumnsForProductFeatureSettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.DropColumns(
                "ProductFeatureSettings",
                "IsPurchaseEnabled",
                "IsAdjustmentEnabled",
                "IsCancellationEnabled",
                "IsRenewalEnabled");
            return Task.FromResult(Unit.Value);
        }

        private void DropColumns(string tableName, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                this.dbContext.ExecuteSqlScript(
                    SqlHelper.DropColumnWithConstraintsIfExists(tableName, columnName));
            }
        }
    }
}
