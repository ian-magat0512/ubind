﻿// <copyright file="MessageEntityProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Model for building an instance of <see cref="MessageEntityProvider"/>.
    /// </summary>
    public class MessageEntityProviderConfigModel : IBuilder<BaseEntityProvider>
    {
        /// <summary>
        /// Gets or sets the message id.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> MessageId { get; set; }

        /// <inheritdoc/>
        public BaseEntityProvider Build(IServiceProvider dependencyProvider)
        {
            var serialisedEntityFactory = dependencyProvider.GetService<ISerialisedEntityFactory>();
            var emailRepository = dependencyProvider.GetService<IEmailRepository>();
            var smsRepository = dependencyProvider.GetService<ISmsRepository>();
            return new MessageEntityProvider(
                this.MessageId?.Build(dependencyProvider), smsRepository, emailRepository, serialisedEntityFactory);
        }
    }
}
