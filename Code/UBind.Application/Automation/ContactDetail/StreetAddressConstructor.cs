// <copyright file="StreetAddressConstructor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.ContactDetail
{
    using System.Threading.Tasks;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    public class StreetAddressConstructor : ContactDetailBase
    {
        public StreetAddressConstructor(
            IProvider<Data<string>> address,
            IProvider<Data<string>> suburb,
            IProvider<Data<string>> state,
            IProvider<Data<string>> postcode,
            IProvider<Data<string>> label,
            IProvider<Data<bool>> condition)
            : base(label, condition)
        {
            this.AddressProvider = address;
            this.SuburbProvider = suburb;
            this.StateProvider = state;
            this.PostcodeProvider = postcode;
        }

        public IProvider<Data<string>> AddressProvider { get; }

        public IProvider<Data<string>> SuburbProvider { get; }

        public IProvider<Data<string>> StateProvider { get; }

        public IProvider<Data<string>> PostcodeProvider { get; }

        public async Task<StreetAddressConstructorData> Resolve(IProviderContext providerContext)
        {
            return new StreetAddressConstructorData(
                (await this.AddressProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.SuburbProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.StateProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.PostcodeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.LabelProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.DefaultProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false);
        }
    }
}
