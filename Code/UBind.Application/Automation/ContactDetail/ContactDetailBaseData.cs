// <copyright file="ContactDetailBaseData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    public abstract class ContactDetailBaseData
    {
        public ContactDetailBaseData(
            string label,
            string value,
            bool isDefault)
        {
            this.Label = label;
            this.IsDefault = isDefault;
            this.Value = value;
        }

        public string Label { get; private set; }

        public string Value { get; private set; }

        public bool IsDefault { get; private set; }

        internal void SetDefaultValue(bool isDefault)
        {
            this.IsDefault = isDefault;
        }
    }
}
