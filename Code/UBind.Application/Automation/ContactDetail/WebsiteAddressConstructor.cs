// <copyright file="WebsiteAddressConstructor.cs" company="uBind">
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
    public class WebsiteAddressConstructor : ContactDetailBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteAddressConstructor"/> class.
        /// </summary>
        public WebsiteAddressConstructor(
            IProvider<Data<string>> websiteAddress,
            IProvider<Data<string>> label,
            IProvider<Data<bool>> @default)
            : base(label, @default)
        {
            this.WebsiteAddressProvider = websiteAddress;
        }

        public IProvider<Data<string>> WebsiteAddressProvider { get; }

        public async Task<WebsiteAddressConstructorData> Resolve(IProviderContext providerContext)
        {
            return new WebsiteAddressConstructorData(
                (await this.WebsiteAddressProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.LabelProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.DefaultProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false);
        }
    }
}
