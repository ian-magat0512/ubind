// <copyright file="CreateRefundCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Accounting
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Accounting;
    using UBind.Domain.Aggregates.Accounting.Refund;
    using UBind.Domain.Commands.Accounting;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Create Payment Command Handler.
    /// </summary>
    public class CreateRefundCommandHandler : ICommandHandler<CreateRefundCommand, Unit>
    {
        private readonly IRefundAggregateRepository refundAggregateRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRefundCommandHandler"/> class.
        /// </summary>
        /// <param name="refundAggregateRepository">The payment aggregate repository.</param>
        /// <param name="clock">The clock.</param>
        public CreateRefundCommandHandler(
            IRefundAggregateRepository refundAggregateRepository,
            IClock clock)
        {
            this.refundAggregateRepository = refundAggregateRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CreateRefundCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ValidateCommand(command);
            var aggregate = RefundAggregate.CreateNewRefund(command.TenantId, command.Amount, command.ReferenceNumber, command.TransactionTime, this.clock.GetCurrentInstant(), command.TransactionParties, command.PerformingUserId);
            await this.refundAggregateRepository.Save(aggregate);

            return Unit.Value;
        }

        /// <summary>
        /// Validate the CreateRefundCommand.
        /// </summary>
        /// <param name="command">The create refund command.</param>
        private void ValidateCommand(CreateRefundCommand command)
        {
            var additionalDetails = new List<string>();

            var errorData = new JObject
            {
                { "commandName", nameof(CreateRefundCommand) },
                { "commandData", JsonConvert.SerializeObject(command) },
            };

            if (command.Amount == 0)
            {
                additionalDetails.Add($"Transaction Time: {command.TransactionTime}");
                throw new ErrorException(Errors.Accounting.RefundCannotBeZero(additionalDetails, errorData));
            }
            else if (command.Amount < 0)
            {
                additionalDetails.Add($"Transaction Time: {command.TransactionTime}");
                throw new ErrorException(Errors.Accounting.RefundCannotBeNegative(command.Amount.Amount, additionalDetails, errorData));
            }
            else if (command.TransactionTime > this.clock.GetCurrentInstant())
            {
                additionalDetails.Add($"Amount: {command.Amount}");
                throw new ErrorException(Errors.Accounting.TransactionDateTimeCannotBeInFuture(command.TransactionTime, "refund", additionalDetails, errorData));
            }
        }
    }
}
