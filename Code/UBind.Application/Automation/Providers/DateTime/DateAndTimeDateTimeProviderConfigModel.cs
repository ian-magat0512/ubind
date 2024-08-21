// <copyright file="DateAndTimeDateTimeProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.DateTime
{
    using System;
    using NodaTime;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents the date and time configuration model defined by a datetime provider.
    /// </summary>
    public class DateAndTimeDateTimeProviderConfigModel : IBuilder<IProvider<Data<Instant>>>
    {
        /// <summary>
        /// Gets or sets the date to be used.
        /// </summary>
        public IBuilder<IProvider<Data<LocalDate>>> Date { get; set; }

        /// <summary>
        /// Gets or sets time to be used.
        /// </summary>
        public IBuilder<IProvider<Data<LocalTime>>> Time { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<Instant>> Build(IServiceProvider dependencyProvider)
        {
            return new DateAndTimeDateTimeProvider(
                this.Date.Build(dependencyProvider),
                this.Time.Build(dependencyProvider));
        }
    }
}
