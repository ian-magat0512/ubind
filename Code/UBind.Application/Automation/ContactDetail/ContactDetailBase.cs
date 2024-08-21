// <copyright file="ContactDetailBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents the the contact details base class, which includes common properties of different contact details like emails, street address,
    /// phone number, social media, and or websites.
    /// </summary>
    public abstract class ContactDetailBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactDetailBase"/> class.
        /// </summary>
        public ContactDetailBase(
            IProvider<Data<string>> label,
            IProvider<Data<bool>> @default)
        {
            this.LabelProvider = label;
            this.DefaultProvider = @default;
        }

        public IProvider<Data<string>> LabelProvider { get; }

        public IProvider<Data<bool>> DefaultProvider { get; }
    }
}
