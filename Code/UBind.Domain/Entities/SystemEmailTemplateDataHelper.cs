// <copyright file="SystemEmailTemplateDataHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace UBind.Domain.Entities
{
    /// <summary>
    /// Helper for system template.
    /// </summary>
    public static class SystemEmailTemplateDataHelper
    {
        public static readonly string ResouceFileLocation = @"UBind.Domain.Resources.SystemEmailTemplate";

        public static readonly string DefaultActivationTitle = @"DefaultActivationTitle";
        public static readonly string DefaultActivationHtmlBody = @"DefaultActivationHtmlBody";
        public static readonly string DefaultActivationPlainTextBody = @"DefaultActivationPlainTextBody";

        public static readonly string DefaultAccountAlreadyActivatedTitle = @"DefaultAccountAlreadyActivatedTitle";
        public static readonly string DefaultAccountAlreadyActivatedHtmlBody = @"DefaultAccountAlreadyActivatedHtmlBody";
        public static readonly string DefaultAccountAlreadyActivatedTextBody = @"DefaultAccountAlreadyActivatedTextBody";

        public static readonly string DefaultPasswordResetTitle = @"DefaultPasswordResetTitle";
        public static readonly string DefaultPasswordResetHtmlBody = @"DefaultPasswordResetHtmlBody";
        public static readonly string DefaultPasswordResetPlainTextBody = @"DefaultPasswordResetPlainTextBody";

        public static readonly string DefaultPasswordExpiredTitle = @"DefaultPasswordExpiredTitle";
        public static readonly string DefaultPasswordExpiredHtmlBody = @"DefaultPasswordExpiredHtmlBody";
        public static readonly string DefaultPasswordExpiredPlainTextBody = @"DefaultPasswordExpiredPlainTextBody";

        public static readonly string DefaultRenewalInvitationTitle = @"DefaultRenewalInvitationTitle";
        public static readonly string DefaultRenewalInvitationHtmlBody = @"DefaultRenewalInvitationHtmlBody";
        public static readonly string DefaultRenewalInvitationTextBody = @"DefaultRenewalInvitationTextBody";

        public static readonly string DefaultQuoteAssociationInvitationTitle = @"DefaultQuoteAssociationInvitationTitle";
        public static readonly string DefaultQuoteAssociationInvitationHtmlBody = @"DefaultQuoteAssociationInvitationHtmlBody";
        public static readonly string DefaultQuoteAssociationInvitationTextBody = @"DefaultQuoteAssociationInvitationTextBody";
    }
}
