import { FeatureSetting } from "@app/models/feature-setting.enum";

export enum Permission {
    ViewMyAccount = "viewMyAccount",
    EditMyAccount = "editMyAccount",
    ViewUsers = "viewUsers",
    ManageUsers = "manageUsers",
    ManageTenantAdminUsers = "manageTenantAdminUsers",
    ManageUBindAdminUsers = "manageUBindAdminUsers",
    ViewRoles = "viewRoles",
    ManageRoles = "manageRoles",
    ViewQuotes = "viewQuotes",
    ManageQuotes = "manageQuotes",
    EndorseQuotes = "endorseQuotes",
    ReviewQuotes = "reviewQuotes",
    ApproveQuotes = "approveQuotes",
    ExportQuotes = "exportQuotes",
    ViewQuoteVersions = "viewQuoteVersions",
    ManageQuoteVersions = "manageQuoteVersions",
    ViewPolicies = "viewPolicies",
    ManagePolicies = "managePolicies",
    ExportPolicies = "exportPolicies",
    ViewClaims = "viewClaims",
    ManageClaims = "manageClaims",
    ExportClaims = "exportClaims",
    AssessClaims = "assessClaims",
    SettleClaims = "settleClaims",
    AcknowledgeClaimNotifications = "acknowledgeClaimNotifications",
    ReviewClaims = "reviewClaims",
    ViewCustomers = "viewCustomers",
    ManageCustomers = "manageCustomers",
    ViewCustomerAccounts = "viewCustomerAccounts",
    ManageCustomerAccounts = "manageCustomerAccounts",
    ViewMessages = "viewMessages",
    ManageMessages = "viewMessages",
    ViewReports = "viewReports",
    ManageReports = "manageReports",
    GenerateReports = "generateReports",
    AccessDevelopmentData = "accessDevelopmentData",
    AccessStagingData = "accessStagingData",
    AccessProductionData = "accessProductionData",
    ViewTenants = "viewTenants",
    ManageTenants = "manageTenants",
    ViewProducts = "viewProducts",
    ManageProducts = "manageProducts",
    ViewReleases = "viewReleases",
    ManageReleases = "manageReleases",
    PromoteReleasesToStaging = "promoteReleasesToStaging",
    PromoteReleasesToProduction = "promoteReleasesToProduction",
    ViewPortals = "viewPortals",
    ManagePortals = "managePortals",
    ViewOrganisations = "viewOrganisations",
    ManageOrganisations = "manageOrganisations",
    ViewAllOrganisations = "viewAllOrganisations",
    ManageAllOrganisations = "manageAllOrganisations",
    ManageOrganisationAdminUsers = "manageOrganisationAdminUsers",
    ViewAdditionalPropertyValues = "viewAdditionalPropertyValues",
    EditAdditionalPropertyValues = "editAdditionalPropertyValues",
    ManageAdditionalPropertyDefinitions = "manageAdditionalPropertyDefinitions",
    ViewAllClaims = "viewAllClaims",
    ViewAllQuotes = "viewAllQuotes",
    ViewAllPolicies = "viewAllPolicies",
    ViewAllCustomers = "viewAllCustomers",
    ViewAllMessages = "viewAllMessages",
    ManageAllClaims = "manageAllClaims",
    ManageAllQuotes = "manageAllQuotes",
    ManageAllPolicies = "manageAllPolicies",
    ManageAllCustomers = "manageAllCustomers",
    ManageAllEmails = "manageAllEmails",
    ViewAllClaimsFromAllOrganisations = "viewAllClaimsFromAllOrganisations",
    ViewAllQuotesFromAllOrganisations = "viewAllQuotesFromAllOrganisations",
    ViewAllPoliciesFromAllOrganisations = "viewAllPoliciesFromAllOrganisations",
    ViewUsersFromOtherOrganisations = "viewUsersFromOtherOrganisations",
    ViewRolesFromAllOrganisations = "viewRolesFromAllOrganisations",
    ManageAllClaimsForAllOrganisations = "manageAllClaimsForAllOrganisations",
    ManageAllQuotesForAllOrganisations = "manageAllQuotesForAllOrganisations",
    ManageAllPoliciesForAllOrganisations = "manageAllPoliciesForAllOrganisations",
    ManageUsersForOtherOrganisations = "manageUsersForOtherOrganisations",
    ManageRolesForAllOrganisations = "manageRolesForAllOrganisations",
    ViewAllCustomersFromAllOrganisations = "viewAllCustomersFromAllOrganisations",
    ManageAllCustomersForAllOrganisations = "manageAllCustomersForAllOrganisations",
    ViewDataTables = "viewDataTables",
    ManageDataTables = "manageDataTables",
    ManagePolicyNumbers = "managePolicyNumbers",
    ManageClaimNumbers = "manageClaimNumbers",
}

/**
 * These are the elevated permissions of a given weaker permission.
 */
export interface ElevatedPermission {
    tenantBoundPermission: Permission;
    organisationBoundPermission: Permission;
    ownershipBoundPermission: Permission;
    // required feature to access the permission.
    requiresFeature?: FeatureSetting;
}

/**
 * This is the structure of the object needed to pass
 * these data to check permission rules with.
 */
export interface PermissionDataModel {
    organisationId: string;
    ownerUserId: string;
    customerId: string;
}
