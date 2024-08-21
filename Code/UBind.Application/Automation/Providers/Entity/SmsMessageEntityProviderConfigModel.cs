// <copyright file="SmsMessageEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Model for building an instance of <see cref="SmsMessageEntityProviderConfigModel"/>.
    /// </summary>
    public class SmsMessageEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the sms id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? SmsId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var smsRepository = dependencyProvider.GetRequiredService<ISmsRepository>();
            return new SmsMessageEntityProvider(
                this.SmsId?.Build(dependencyProvider),
                smsRepository,
                serialisedEntityFactory);
        }
    }
}
