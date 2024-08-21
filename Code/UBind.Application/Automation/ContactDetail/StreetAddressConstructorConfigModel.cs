// <copyright file="StreetAddressConstructorConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    public class StreetAddressConstructorConfigModel : IBuilder<StreetAddressConstructor>
    {
        [JsonConstructor]
        public StreetAddressConstructorConfigModel(
            IBuilder<IProvider<Data<string>>> address,
            IBuilder<IProvider<Data<string>>> suburb,
            IBuilder<IProvider<Data<string>>> state,
            IBuilder<IProvider<Data<string>>> postcode,
            IBuilder<IProvider<Data<string>>> label,
            IBuilder<IProvider<Data<bool>>> @default)
        {
            this.Address = address;
            this.Suburb = suburb;
            this.State = state;
            this.Postcode = postcode;
            this.Label = label;
            this.Default = @default;
        }

        [JsonProperty("address")]
        public IBuilder<IProvider<Data<string>>> Address { get; set; }

        [JsonProperty("suburb")]
        public IBuilder<IProvider<Data<string>>> Suburb { get; set; }

        [JsonProperty("state")]
        public IBuilder<IProvider<Data<string>>> State { get; set; }

        [JsonProperty("postcode")]
        public IBuilder<IProvider<Data<string>>> Postcode { get; set; }

        [JsonProperty("label")]
        public IBuilder<IProvider<Data<string>>> Label { get; set; }

        [JsonProperty("default")]
        public IBuilder<IProvider<Data<bool>>> Default { get; set; }

        /// <inheritdoc/>
        public StreetAddressConstructor Build(IServiceProvider dependencyProvider)
        {
            return new StreetAddressConstructor(
                this.Address.Build(dependencyProvider),
                this.Suburb.Build(dependencyProvider),
                this.State.Build(dependencyProvider),
                this.Postcode.Build(dependencyProvider),
                this.Label.Build(dependencyProvider),
                this.Default?.Build(dependencyProvider));
        }
    }
}
