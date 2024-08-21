// <copyright file="EmailAddressConstructor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    using System.Threading.Tasks;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    public class EmailAddressConstructor : ContactDetailBase
    {
        public EmailAddressConstructor(
            IProvider<Data<string>> emailAddress,
            IProvider<Data<string>> label,
            IProvider<Data<bool>> @default)
            : base(label, @default)
        {
            this.EmailAddressProvider = emailAddress;
        }

        public IProvider<Data<string>> EmailAddressProvider { get; }

        public async Task<EmailAddressConstructorData> Resolve(IProviderContext providerContext)
        {
            return new EmailAddressConstructorData(
                (await this.EmailAddressProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.LabelProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.DefaultProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false);
        }
    }
}
