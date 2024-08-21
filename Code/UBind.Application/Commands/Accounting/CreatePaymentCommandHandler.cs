// <copyright file="CreatePaymentCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Aggregates.Accounting.Payment;
    using UBind.Domain.Commands.Accounting;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Create Payment Command Handler.
    /// </summary>
    public class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, Unit>
    {
        private readonly ICqrsMediator mediator;
        private readonly IPaymentAggregateRepository paymentAggregateRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePaymentCommandHandler"/> class.
        /// </summary>
        /// <param name="mediator">The mediator service factory.</param>
        /// <param name="paymentAggregateRepository">The payment aggregate repository.</param>
        /// <param name="clock">The clock.</param>
        public CreatePaymentCommandHandler(
            ICqrsMediator mediator,
            IPaymentAggregateRepository paymentAggregateRepository,
            IClock clock)
        {
            this.mediator = mediator;
            this.paymentAggregateRepository = paymentAggregateRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ValidateCommand(command);

            var aggregate = PaymentAggregate.CreateNewPayment(command.TenantId, command.Amount, command.ReferenceNumber, command.TransactionTime, this.clock.GetCurrentInstant(), command.TransactionParties, command.PerformingUserId);
            await this.paymentAggregateRepository.Save(aggregate);

            return Unit.Value;
        }

        /// <summary>
        /// Validate the create payment command.
        /// </summary>
        /// <param name="command">The command.</param>
        private void ValidateCommand(CreatePaymentCommand command)
        {
            var additionalDetails = new List<string>();

            var errorData = new JObject
            {
                { "commandName", nameof(CreatePaymentCommand) },
                { "commandData", JsonConvert.SerializeObject(command) },
            };

            if (command.Amount == 0)
            {
                additionalDetails.Add($"Transaction Time: {command.TransactionTime}");
                throw new ErrorException(Errors.Accounting.PaymentCannotBeZero(additionalDetails, errorData));
            }
            else if (command.Amount < 0)
            {
                additionalDetails.Add($"Transaction Time: {command.TransactionTime}");
                throw new ErrorException(Errors.Accounting.PaymentCannotBeNegative(command.Amount.Amount, additionalDetails, errorData));
            }
            else if (command.TransactionTime > this.clock.GetCurrentInstant())
            {
                additionalDetails.Add($"Amount: {command.Amount}");
                throw new ErrorException(Errors.Accounting.TransactionDateTimeCannotBeInFuture(command.TransactionTime, "payment", additionalDetails, errorData));
            }
        }
    }
}
