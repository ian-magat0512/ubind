// <copyright file="AdditionalPropertyValueTextProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Config model class of additional property value text.
    /// </summary>
    public class AdditionalPropertyValueTextProviderConfigModel : IBuilder<IProvider<Data<string?>>>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        public IBuilder<BaseEntityProvider> Entity { get; set; }

        /// <summary>
        /// Gets or sets the property alias of the additional property definition to look for.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> PropertyAlias { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the value if not set.
        /// </summary>
        public IBuilder<IProvider<Data<string?>>>? ValueIfNotSet { get; set; }

        public IBuilder<IProvider<Data<bool>>>? RaiseErrorIfNotDefined { get; set; }

        public IBuilder<IProvider<Data<string?>>>? ValueIfNotDefined { get; set; }

        public IBuilder<IProvider<Data<bool>>>? RaiseErrorIfNotSet { get; set; }

        public IBuilder<IProvider<Data<bool>>>? RaiseErrorIfTypeMismatch { get; set; }

        public IBuilder<IProvider<Data<string?>>>? ValueIfTypeMismatch { get; set; }

        public IBuilder<IProvider<Data<string?>>>? DefaultValue { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<string?>> Build(IServiceProvider dependencyProvider)
        {
            var textAdditionalPropertyValueRepository =
                dependencyProvider.GetRequiredService<ITextAdditionalPropertyValueReadModelRepository>();

            var additionalPropertyDefinitionRepository =
                dependencyProvider.GetRequiredService<IAdditionalPropertyDefinitionRepository>();

            var structuredDataAdditionalPropertyValueRepository =
                dependencyProvider.GetRequiredService<IStructuredDataAdditionalPropertyValueReadModelRepository>();

            return new AdditionalPropertyValueTextProvider(
                this.Entity.Build(dependencyProvider),
                this.PropertyAlias.Build(dependencyProvider),
                this.ValueIfNotSet?.Build(dependencyProvider),
                this.RaiseErrorIfNotDefined?.Build(dependencyProvider),
                this.ValueIfNotDefined?.Build(dependencyProvider),
                this.RaiseErrorIfNotSet?.Build(dependencyProvider),
                this.RaiseErrorIfTypeMismatch?.Build(dependencyProvider),
                this.ValueIfTypeMismatch?.Build(dependencyProvider),
                this.DefaultValue?.Build(dependencyProvider),
                textAdditionalPropertyValueRepository,
                additionalPropertyDefinitionRepository,
                structuredDataAdditionalPropertyValueRepository);
        }
    }
}
