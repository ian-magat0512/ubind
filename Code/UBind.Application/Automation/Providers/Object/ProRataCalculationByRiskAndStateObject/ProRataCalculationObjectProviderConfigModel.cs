// <copyright file="ProRataCalculationObjectProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model for building an instance of <see cref="ProRataCalculationObjectProvider"/>.
    /// </summary>
    public class ProRataCalculationObjectProviderConfigModel : IBuilder<IObjectProvider>
    {
        public IBuilder<IProvider<Data<string>>> PolicyTransactionId { get; set; }

        /// <inheritdoc/>
        public IObjectProvider Build(IServiceProvider dependencyProvider)
        {
            var repository = dependencyProvider.GetService<IPolicyTransactionReadModelRepository>();
            var policyTransactionId = this.PolicyTransactionId.Build(dependencyProvider);
            if (policyTransactionId == null)
            {
                var errorData = new JObject
                {
                    { ErrorDataKey.EntityType, "policyTransaction" },
                    { "policyTransactionId", null },
                };
                throw new ErrorException(
                    Errors.Automation.Provider.Entity.NotFound(
                        EntityType.PolicyTransaction.Humanize(), "policyTransactionId", "Null", errorData));
            }

            return new ProRataCalculationObjectProvider(policyTransactionId, repository);
        }
    }
}
