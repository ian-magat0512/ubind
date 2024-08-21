// <copyright file="RenameDateFieldsData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System.Collections.Generic;
    using UBind.Persistence.Migrations.Helpers;

    public static class RenameDateFieldsData
    {
        public static List<ColumnRenameModel> GetTimestampColumnRenames()
        {
            var columnRenames = new List<ColumnRenameModel>
            {
                new ColumnRenameModel("StartupJobs", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Tenants", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("TenantDetails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Deployments", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("DevReleases", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Releases", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ReleaseDescriptions", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ReleaseEvents", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ReleaseDetails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("DeploymentTargetDetails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("DeploymentTargets", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Portals", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PortalDetails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PortalSettings", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ReportFiles", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Roles", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("UserReadModels", "PasswordLastChangedTimeInTicksSinceEpoch", "PasswordLastChangedTicksSinceEpoch"),
                new ColumnRenameModel("UserReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("UserReadModels", "LastModifiedTimeInTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("SystemAlerts", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("TextAdditionalPropertyValueReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("TokenSessions", "LastTimeUsedInTicksSinceEpoch", "LastUsedTicksSinceEpoch"),
                new ColumnRenameModel("TokenSessions", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("UniqueIdentifiers", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("UniqueIdentifierConsumptions", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("UserEmailReadModels", "CreationTimeAsTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("UserEmailSentReadModels", "CreationTimeAsTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("UserLoginEmails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PortalSettingDetails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Settings", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("SettingDetails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ProductFeatureSettings", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ProductOrganisationSettings", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ProductPortalSettings", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Products", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ProductDetails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ProductEvents", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("AdditionalPropertyDefinitions", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("AdditionalPropertyDefinitions", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ClaimFileAttachments", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ClaimNumbers", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ClaimReadModels", "CreationTimeAsTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ClaimReadModels", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("ClaimVersionReadModels", "CreationTimeTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ClaimVersionReadModels", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("CreditNoteNumbers", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("CustomerReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("CustomerReadModels", "LastModifiedTimeInTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("PersonReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PersonReadModels", "LastModifiedInTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("Assets", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Files", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("DkimSettings", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("DocumentFiles", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("EmailAddressBlockingEvents", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Emails", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("EmailAttachments", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("SystemEmailTemplates", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("InvoiceNumbers", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("LoginAttemptResults", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("OrganisationReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PasswordResetRecords", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ApplicationPasswords", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PaymentAllocationReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Invoices", "DueDateTimeInTicksSinceEpoch", "DueTicksSinceEpoch"),
                new ColumnRenameModel("Invoices", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PaymentReadModels", "TransactionTimeAsTicksSinceEpoch", "TransactionTicksSinceEpoch"),
                new ColumnRenameModel("PaymentReadModels", "CreationTimeAsTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PaymentReadModels", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("PolicyReadModels", "PolicyIssueTimeInTicksSinceEpoch", "IssuedTicksSinceEpoch"),
                new ColumnRenameModel("PolicyReadModels", "InceptionTimeAsTicksSinceEpoch", "InceptionTicksSinceEpoch"),
                new ColumnRenameModel("PolicyReadModels", "ExpiryTimeAsTicksSinceEpoch", "ExpiryTicksSinceEpoch", true),
                new ColumnRenameModel("PolicyReadModels", "CancellationEffectiveTimeInTicksSinceEpoch", "CancellationEffectiveTicksSinceEpoch", true),
                new ColumnRenameModel("PolicyReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PolicyReadModels", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("PolicyNumbers", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("PolicyTransactions", "PolicyData_ExpiryTimeInTicksSinceEpoch", "ExpiryTicksSinceEpoch", true),
                new ColumnRenameModel("PolicyTransactions", "EffectiveTimeInTicksSinceEpoch", "EffectiveTicksSinceEpoch"),
                new ColumnRenameModel("PolicyTransactions", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("ReportReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("QuoteDocumentReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("QuoteEmailReadModels", "CreationTimeAsTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("QuoteEmailSendingReadModels", "CreationTimeAsTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("QuoteFileAttachments", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "ExpiryTimeAsTicksSinceEpoch", "ExpiryTicksSinceEpoch", true),
                new ColumnRenameModel("Quotes", "InvoiceTimeAsTicksSinceEpoch", "InvoiceTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "SubmissionTimeAsTicksSinceEpoch", "SubmissionTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "PaymentTimeAsTicksSinceEpoch", "PaymentTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "FundingTimeAsTicksSinceEpoch", "FundingTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "CreditNoteTimeAsTicksSinceEpoch", "CreditNoteTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Quotes", "LastUpdatedByUserTicksSinceEpoch", "LastModifiedByUserTicksSinceEpoch", true),
                new ColumnRenameModel("QuoteVersionReadModels", "CreationTimeTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("QuoteVersionReadModels", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("RefundAllocationReadModels", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("CreditNotes", "DueDateTimeInTicksSinceEpoch", "DueTicksSinceEpoch"),
                new ColumnRenameModel("CreditNotes", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("RefundReadModels", "TransactionTimeAsTicksSinceEpoch", "TransactionTicksSinceEpoch"),
                new ColumnRenameModel("RefundReadModels", "CreationTimeAsTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("RefundReadModels", "LastUpdatedTicksSinceEpoch", "LastModifiedTicksSinceEpoch"),
                new ColumnRenameModel("Sms", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Tags", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),

                 // The execution of these two will be defered to another ticket UB-8559,
                 // the current implementation will only update the past day records.
                new ColumnRenameModel("SystemEvents", "ExpiryTimeAsTicksSinceEpoch", "ExpiryTicksSinceEpoch"),
                new ColumnRenameModel("SystemEvents", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
                new ColumnRenameModel("Relationships", "CreationTimeInTicksSinceEpoch", "CreatedTicksSinceEpoch"),
            };

            return columnRenames;
        }

        public static List<ColumnRenameModel> GetDateTimeColumnRenames()
        {
            var columnRenames = new List<ColumnRenameModel>
            {
                new ColumnRenameModel("PolicyReadModels", "InceptionDateAsDateTime", "InceptionDateTime")
                {
                    OldStoreType = "datetime",
                    NewStoreType = "datetime2",
                    NewPrecision = 7,
                },
                new ColumnRenameModel("PolicyReadModels", "CancellationDateAsDateTime", "CancellationEffectiveDateTime", true)
                {
                    OldStoreType = "datetime",
                    NewStoreType = "datetime2",
                    NewPrecision = 7,
                },
                new ColumnRenameModel("PolicyReadModels", "ExpiryDateAsDateTime", "ExpiryDateTime", true)
                {
                    OldStoreType = "datetime",
                    NewStoreType = "datetime2",
                    NewPrecision = 7,
                },
                new ColumnRenameModel("PolicyTransactions", "PolicyData_ExpiryDateAsDateTime", "ExpiryDateTime", true),
            };

            return columnRenames;
        }
    }
}
