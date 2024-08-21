// <copyright file="MessengerIdConstructorConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    public class MessengerIdConstructorConfigModel : IBuilder<MessengerIdConstructor>
    {
        [JsonConstructor]
        public MessengerIdConstructorConfigModel(
            IBuilder<IProvider<Data<string>>> messengerId,
            IBuilder<IProvider<Data<string>>> label,
            IBuilder<IProvider<Data<bool>>> @default)
        {
            this.MessengerId = messengerId;
            this.Label = label;
            this.Default = @default;
        }

        [JsonProperty("messengerId")]
        public IBuilder<IProvider<Data<string>>> MessengerId { get; set; }

        [JsonProperty("label")]
        public IBuilder<IProvider<Data<string>>> Label { get; set; }

        [JsonProperty("default")]
        public IBuilder<IProvider<Data<bool>>> Default { get; set; }

        /// <inheritdoc/>
        public MessengerIdConstructor Build(IServiceProvider dependencyProvider)
        {
            var @default = this.Default?.Build(dependencyProvider);
            return new MessengerIdConstructor(
                this.MessengerId.Build(dependencyProvider),
                this.Label.Build(dependencyProvider),
                @default);
        }
    }
}
