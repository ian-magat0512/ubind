// <copyright file="UpdateProductFeatureRenewalSettingCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;

    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    /// <summary>
    /// command handler for updating product feature renewal setting.
    /// </summary>
    public class UpdateProductFeatureRenewalSettingCommandHandler : ICommandHandler<UpdateProductFeatureRenewalSettingCommand, Unit>
    {
        private readonly IProductFeatureSettingRepository productFeatureRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProductFeatureRenewalSettingCommandHandler"/> class.
        /// </summary>
        /// <param name="productFeatureRepository">The product feature repository.</param>
        public UpdateProductFeatureRenewalSettingCommandHandler(
            IProductFeatureSettingRepository productFeatureRepository)
        {
            this.productFeatureRepository = productFeatureRepository;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(UpdateProductFeatureRenewalSettingCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Duration duration = Duration.FromSeconds(command.ExpiredPolicyRenewalPeriodSeconds);
            if (duration.Days < 0 || duration.Days > 365)
            {
                throw new ErrorException(Errors.ProductFeatureSetting.IncorrectExpiredPolicyRenewalDuration(duration.Days));
            }

            this.productFeatureRepository.UpdateProductFeatureRenewalSetting(
                command.TenantId,
                command.ProductId,
                command.AllowRenewalAfterExpiry,
                duration);

            return Task.FromResult(Unit.Value);
        }
    }
}
