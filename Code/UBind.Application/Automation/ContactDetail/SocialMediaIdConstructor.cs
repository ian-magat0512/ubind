// <copyright file="SocialMediaIdConstructor.cs" company="uBind">
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
    public class SocialMediaIdConstructor : ContactDetailBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerIdConstructor"/> class.
        /// </summary>
        public SocialMediaIdConstructor(
            IProvider<Data<string>> socialMediaId,
            IProvider<Data<string>> label,
            IProvider<Data<bool>> condition)
            : base(label, condition)
        {
            this.SocialMediaIdProvider = socialMediaId;
        }

        public IProvider<Data<string>> SocialMediaIdProvider { get; }

        public async Task<SocialMediaIdConstructorData> Resolve(IProviderContext providerContext)
        {
            return new SocialMediaIdConstructorData(
                (await this.SocialMediaIdProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.LabelProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.DefaultProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false);
        }
    }
}
