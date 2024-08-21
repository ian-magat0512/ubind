// <copyright file="MessengerIdConstructor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    using System.Threading.Tasks;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents the definition of the API endpoint that the HTTP trigger will be invoked through.
    /// </summary>
    public class MessengerIdConstructor : ContactDetailBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerIdConstructor"/> class.
        /// </summary>
        public MessengerIdConstructor(
            IProvider<Data<string>> messengerId,
            IProvider<Data<string>> label,
            IProvider<Data<bool>> @default)
            : base(label, @default)
        {
            this.MessengerIdProvider = messengerId;
        }

        public IProvider<Data<string>> MessengerIdProvider { get; }

        public async Task<MessengerIdConstructorData> Resolve(IProviderContext providerContext)
        {
            return new MessengerIdConstructorData(
                (await this.MessengerIdProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.LabelProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue,
                (await this.DefaultProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false);
        }
    }
}
