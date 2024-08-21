// <copyright file="EmailDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using global::DotLiquid;
    using UBind.Domain;

    /// <summary>
    /// View model for mail messaging service.
    /// </summary>
    public class EmailDrop : Drop
    {
        public EmailDrop(
            Guid tenantId,
            Guid? productId,
            Guid? portalId,
            SystemEmailType emailType,
            UserDrop userDrop,
            TenantDrop tenantDrop,
            OrganisationDrop organisationDrop,
            EmailInvitationDrop passwordResetDrop,
            EmailInvitationDrop passwordExpiredResetDrop,
            EmailInvitationDrop userActivationDrop,
            PolicyDrop policyDrop,
            PolicyRenewalDrop policyRenewalDrop,
            QuoteCustomerAssociationDrop quoteCustomerAssociationDrop,
            PersonDrop person)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.PortalId = portalId;
            this.EmailType = emailType;
            this.User = userDrop;
            this.Tenant = tenantDrop;
            this.Organisation = organisationDrop;
            this.PasswordReset = passwordResetDrop;
            this.PasswordExpiredReset = passwordExpiredResetDrop;
            this.UserActivation = userActivationDrop;
            this.Policy = policyDrop;
            this.PolicyRenewal = policyRenewalDrop;
            this.QuoteCustomerAssociation = quoteCustomerAssociationDrop;
            this.Person = person;
        }

        /// <summary>
        /// Gets or sets gets the User Drop.
        /// </summary>
        public UserDrop User { get; set; }

        /// <summary>
        /// Gets or sets gets the Person Drop.
        /// </summary>
        public PersonDrop Person { get; set; }

        /// <summary>
        /// Gets or sets the Tenant Drop.
        /// </summary>
        public TenantDrop Tenant { get; set; }

        /// <summary>
        /// Gets or sets the Organisation Drop.
        /// </summary>
        public OrganisationDrop Organisation { get; set; }

        /// <summary>
        /// Gets or sets the password reset Drop.
        /// </summary>
        public EmailInvitationDrop PasswordReset { get; set; }

        /// <summary>
        /// Gets or sets the password expired reset Drop.
        /// </summary>
        public EmailInvitationDrop PasswordExpiredReset { get; set; }

        /// <summary>
        /// Gets or sets the password reset Drop.
        /// </summary>
        public EmailInvitationDrop UserActivation { get; set; }

        /// <summary>
        /// Gets or sets the Policy Drop.
        /// </summary>
        public PolicyDrop Policy { get; set; }

        /// <summary>
        /// Gets or sets the Policy Renewal Drop.
        /// </summary>
        public PolicyRenewalDrop PolicyRenewal { get; set; }

        /// <summary>
        /// Gets or sets the Quote Association Drop.
        /// </summary>
        public QuoteCustomerAssociationDrop QuoteCustomerAssociation { get; set; }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the product ID.
        /// </summary>
        public Guid? ProductId { get; }

        public Guid? PortalId { get; }

        /// <summary>
        /// Gets the system email type.
        /// </summary>
        public SystemEmailType EmailType { get; }

        /// <summary>
        /// Create a Password Reset invation model.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="userDrop">The user drop.</param>
        /// <param name="tenantDrop">The tenant drop.</param>
        /// <param name="organisationDrop">The organisation drop.</param>
        /// <param name="passwordResetDrop">The password reset drop.</param>
        /// <returns>DotLiquidMessagingModel.</returns>
        public static EmailDrop CreatePasswordResetInvition(
            Guid tenantId,
            Guid? portalId,
            UserDrop userDrop,
            TenantDrop tenantDrop,
            OrganisationDrop organisationDrop,
            EmailInvitationDrop passwordResetDrop)
        {
            return new EmailDrop(
                tenantId,
                null,
                portalId,
                SystemEmailType.PasswordResetInvitation,
                userDrop,
                tenantDrop,
                organisationDrop,
                passwordResetDrop,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        /// <summary>
        /// Create a Password Expired Reset invation model.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="userDrop">The user drop.</param>
        /// <param name="tenantDrop">The tenant drop.</param>
        /// <param name="organisationDrop">The organisation drop.</param>
        /// <param name="passwordExpiredResetDrop">The password expired reset drop.</param>
        /// <returns>DotLiquidMessagingModel.</returns>
        public static EmailDrop CreatePasswordExpiredResetInvition(
            Guid tenantId,
            Guid? productId,
            Guid? portalId,
            UserDrop userDrop,
            TenantDrop tenantDrop,
            OrganisationDrop organisationDrop,
            EmailInvitationDrop passwordExpiredResetDrop)
        {
            return new EmailDrop(
                tenantId,
                productId,
                portalId,
                SystemEmailType.PasswordExpiredResetInvitation,
                userDrop,
                tenantDrop,
                organisationDrop,
                null,
                passwordExpiredResetDrop,
                null,
                null,
                null,
                null,
                null);
        }

        /// <summary>
        /// Create a User Activation invitation model.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="userDrop">The user drop.</param>
        /// <param name="tenantDrop">The tenant drop.</param>
        /// <param name="organisationDrop">The organisation drop.</param>
        /// <param name="userActivationDrop">The user activation drop.</param>
        /// <returns>DotLiquidMessagingModel.</returns>
        public static EmailDrop CreateUserActivationInvitation(
            Guid tenantId,
            Guid? portalId,
            UserDrop userDrop,
            TenantDrop tenantDrop,
            OrganisationDrop organisationDrop,
            EmailInvitationDrop userActivationDrop)
        {
            return new EmailDrop(
                tenantId,
                null,
                portalId,
                SystemEmailType.AccountActivationInvitation,
                userDrop,
                tenantDrop,
                organisationDrop,
                null,
                null,
                userActivationDrop,
                null,
                null,
                null,
                null);
        }

        /// <summary>
        /// Create Account Already Activated model.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="userDrop">The user drop.</param>
        /// <param name="tenantDrop">The tenant drop.</param>
        /// <param name="organisationDrop">The organisation drop.</param>
        /// <returns>DotLiquidMessagingModel.</returns>
        public static EmailDrop CreateAccountAlreadyActivated(
            Guid tenantId,
            Guid? portalId,
            UserDrop userDrop,
            TenantDrop tenantDrop,
            OrganisationDrop organisationDrop)
        {
            return new EmailDrop(
                tenantId,
                null,
                portalId,
                SystemEmailType.AccountAlreadyActivated,
                userDrop,
                tenantDrop,
                organisationDrop,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        /// <summary>
        /// Create a Policy renewal invitation model.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="tenantDrop">The tenant drop.</param>
        /// <param name="policyDrop">The policy drop.</param>
        /// <param name="organisationDrop">The organisation drop.</param>
        /// <param name="policyRenewalDrop">The policy renewal drop.</param>
        /// <param name="userDrop">The user drop.</param>
        /// <param name="personDrop">The peson drop.</param>
        /// <returns>DotLiquidMessagingModel.</returns>
        public static EmailDrop CreatePolicyRenewalInvation(
            Guid tenantId,
            Guid productId,
            Guid? portalId,
            TenantDrop tenantDrop,
            PolicyDrop policyDrop,
            OrganisationDrop organisationDrop,
            PolicyRenewalDrop policyRenewalDrop,
            UserDrop userDrop,
            PersonDrop personDrop)
        {
            return new EmailDrop(
                tenantId,
                productId,
                portalId,
                SystemEmailType.RenewalInvitation,
                userDrop,
                tenantDrop,
                organisationDrop,
                null,
                null,
                null,
                policyDrop,
                policyRenewalDrop,
                null,
                personDrop);
        }

        /// <summary>
        /// Create a Quote Association invitation model.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="userDrop">The user drop.</param>
        /// <param name="tenantDrop">The tenant drop.</param>
        /// <param name="organisationDrop">The organisation drop.</param>
        /// <param name="quoteCustomerAssociationDrop">The quote association drop.</param>
        /// <returns>DotLiquidMessagingModel.</returns>
        public static EmailDrop CreateQuoteAssociationInvitation(
            Guid tenantId,
            Guid productId,
            Guid? portalId,
            UserDrop userDrop,
            TenantDrop tenantDrop,
            OrganisationDrop organisationDrop,
            QuoteCustomerAssociationDrop quoteCustomerAssociationDrop)
        {
            return new EmailDrop(
                tenantId,
                productId,
                portalId,
                SystemEmailType.QuoteAssociationInvitation,
                userDrop,
                tenantDrop,
                organisationDrop,
                null,
                null,
                null,
                null,
                null,
                quoteCustomerAssociationDrop,
                null);
        }

        /// <summary>
        /// Mask invitation IDs and links from the drop (so we don't persist them and expose them in the portal).
        /// </summary>
        public void MaskInvitationLink()
        {
            if (this.PasswordReset != null)
            {
                this.PasswordReset = EmailInvitationDrop.CreateMaskedPasswordResetDrop();
            }

            if (this.PasswordExpiredReset != null)
            {
                this.PasswordExpiredReset = EmailInvitationDrop.CreateMaskedPasswordExpiredResetDrop();
            }

            if (this.UserActivation != null)
            {
                this.UserActivation = EmailInvitationDrop.CreateMaskedAccountActivationDrop();
            }

            if (this.QuoteCustomerAssociation != null)
            {
                this.QuoteCustomerAssociation = QuoteCustomerAssociationDrop.CreateMaskedDrop();
            }
        }
    }
}
