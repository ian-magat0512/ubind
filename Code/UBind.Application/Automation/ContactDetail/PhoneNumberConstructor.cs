// <copyright file="PhoneNumberConstructor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.ContactDetail
{
    using System.Threading.Tasks;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents the definition of the API endpoint that the HTTP trigger will be invoked through.
    /// </summary>
    public class PhoneNumberConstructor : ContactDetailBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberConstructor"/> class.
        /// </summary>
        public PhoneNumberConstructor(
            IProvider<Data<string>> phoneNumber,
            IProvider<Data<string>> label,
            IProvider<Data<bool>> @default)
            : base(label, @default)
        {
            this.PhoneNumberProvider = phoneNumber;
        }

        public IProvider<Data<string>> PhoneNumberProvider { get; }

        public async Task<PhoneNumberConstructorData> Resolve(IProviderContext providerContext)
        {
            return new PhoneNumberConstructorData(
                (await this.PhoneNumberProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.LabelProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.DefaultProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false);
        }
    }
}
