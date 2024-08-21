// <copyright file="PatchObjectProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class is needed for creating an instance of <see cref="PatchObjectProvider"/> from the JSON configuration.
    /// </summary>
    public class PatchObjectProviderConfigModel : IBuilder<IObjectProvider>
    {
        public PatchObjectProviderConfigModel(
            IBuilder<IObjectProvider> obj,
            IEnumerable<IBuilder<BaseOperation>> operations,
            IBuilder<IObjectProvider> valueIfAborted)
        {
            this.Object = obj;
            this.Operations = operations;
            this.ValueIfAborted = valueIfAborted;
        }

        public IBuilder<IObjectProvider> Object { get; set; }

        public IEnumerable<IBuilder<BaseOperation>> Operations { get; set; } = Enumerable.Empty<IBuilder<BaseOperation>>();

        public IBuilder<IObjectProvider> ValueIfAborted { get; set; }

        public IObjectProvider Build(IServiceProvider dependencyProvider)
        {
            var operations = new List<BaseOperation>();
            foreach (var operation in this.Operations)
            {
                operations.Add(operation.Build(dependencyProvider));
            }

            var obj = this.Object.Build(dependencyProvider);
            var valueIfAborted = this.ValueIfAborted?.Build(dependencyProvider);
            return new PatchObjectProvider(obj, operations, valueIfAborted);
        }
    }
}
