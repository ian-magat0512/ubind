// <copyright file="EmailMessageEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Model for building an instance of <see cref="EmailMessageEntityProvider"/>.
    /// </summary>
    public class EmailMessageEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the email id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? EmailId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
            var emailRepository = dependencyProvider.GetRequiredService<IEmailRepository>();
            return new EmailMessageEntityProvider(this.EmailId?.Build(dependencyProvider), emailRepository, serialisedEntityFactory);
        }
    }
}
