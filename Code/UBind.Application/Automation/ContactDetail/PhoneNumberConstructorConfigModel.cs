// <copyright file="PhoneNumberConstructorConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    public class PhoneNumberConstructorConfigModel : IBuilder<PhoneNumberConstructor>
    {
        [JsonConstructor]
        public PhoneNumberConstructorConfigModel(
            IBuilder<IProvider<Data<string>>> phoneNumber,
            IBuilder<IProvider<Data<string>>> label,
            IBuilder<IProvider<Data<bool>>> @default)
        {
            this.PhoneNumber = phoneNumber;
            this.Label = label;
            this.Default = @default;
        }

        [JsonProperty("phoneNumber")]
        public IBuilder<IProvider<Data<string>>> PhoneNumber { get; set; }

        [JsonProperty("label")]
        public IBuilder<IProvider<Data<string>>> Label { get; set; }

        [JsonProperty("default")]
        public IBuilder<IProvider<Data<bool>>> Default { get; set; }

        /// <inheritdoc/>
        public PhoneNumberConstructor Build(IServiceProvider dependencyProvider)
        {
            return new PhoneNumberConstructor(
                this.PhoneNumber.Build(dependencyProvider),
                this.Label.Build(dependencyProvider),
                this.Default?.Build(dependencyProvider));
        }
    }
}
