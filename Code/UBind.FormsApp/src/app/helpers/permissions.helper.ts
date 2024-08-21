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
}

/**
 * These are the elevated permissions of a given weaker permission.
 */
export interface ElevatedPermission {
    tenantBoundPermission: Permission;
    organisationBoundPermission: Permission;
    ownershipBoundPermission: Permission;
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

// easily identify other elevated permissions of a permission.
export const elevatedPermissionMapping: { [key: string]: ElevatedPermission } = {
    manageUsers: {
        ownershipBoundPermission: null,
        organisationBoundPermission: Permission.ManageUsers,
        tenantBoundPermission: Permission.ManageUsersForOtherOrganisations,
    },
    manageQuotes: {
        ownershipBoundPermission: Permission.ManageQuotes,
        organisationBoundPermission: Permission.ManageAllQuotes,
        tenantBoundPermission: Permission.ManageAllQuotesForAllOrganisations,
    },
    managePolicies: {
        ownershipBoundPermission: Permission.ManagePolicies,
        organisationBoundPermission: Permission.ManageAllPolicies,
        tenantBoundPermission: Permission.ManageAllPoliciesForAllOrganisations,
    },
    manageCustomers: {
        ownershipBoundPermission: Permission.ManageCustomers,
        organisationBoundPermission: Permission.ManageAllCustomers,
        tenantBoundPermission: null,
    },
    manageClaims: {
        ownershipBoundPermission: Permission.ManageClaims,
        organisationBoundPermission: Permission.ManageAllClaims,
        tenantBoundPermission: Permission.ManageAllClaimsForAllOrganisations,
    },
    manageRoles: {
        ownershipBoundPermission: null,
        organisationBoundPermission: Permission.ManageRoles,
        tenantBoundPermission: Permission.ManageRolesForAllOrganisations,
    },
    viewUsers: {
        ownershipBoundPermission: null,
        organisationBoundPermission: Permission.ViewUsers,
        tenantBoundPermission: Permission.ViewUsersFromOtherOrganisations,
    },
    viewQuotes: {
        ownershipBoundPermission: Permission.ViewQuotes,
        organisationBoundPermission: Permission.ViewAllQuotes,
        tenantBoundPermission: Permission.ViewAllQuotesFromAllOrganisations,
    },
    viewPolicies: {
        ownershipBoundPermission: Permission.ViewPolicies,
        organisationBoundPermission: Permission.ViewAllPolicies,
        tenantBoundPermission: Permission.ViewAllPoliciesFromAllOrganisations,
    },
    viewCustomers: {
        ownershipBoundPermission: Permission.ViewCustomers,
        organisationBoundPermission: Permission.ViewAllCustomers,
        tenantBoundPermission: null,
    },
    viewClaims: {
        ownershipBoundPermission: Permission.ViewClaims,
        organisationBoundPermission: Permission.ViewAllClaims,
        tenantBoundPermission: Permission.ViewAllClaimsFromAllOrganisations,
    },
    viewRoles: {
        ownershipBoundPermission: null,
        organisationBoundPermission: Permission.ViewRoles,
        tenantBoundPermission: Permission.ViewRolesFromAllOrganisations,
    },
    manageOrganisations: {
        ownershipBoundPermission: null,
        organisationBoundPermission: null,
        tenantBoundPermission: Permission.ManageOrganisations,
    },
    viewOrganisations: {
        ownershipBoundPermission: null,
        organisationBoundPermission: null,
        tenantBoundPermission: Permission.ViewOrganisations,
    },
    viewPortals: {
        ownershipBoundPermission: null,
        organisationBoundPermission: null,
        tenantBoundPermission: Permission.ViewPortals,
    },
    managePortals: {
        ownershipBoundPermission: null,
        organisationBoundPermission: null,
        tenantBoundPermission: Permission.ManagePortals,
    },
    viewMessages: {
        ownershipBoundPermission: Permission.ViewMessages,
        organisationBoundPermission: Permission.ViewAllMessages,
        tenantBoundPermission: null, // tenant bound has no requirements.
    },
    manageMessages: {
        ownershipBoundPermission: Permission.ManageMessages,
        organisationBoundPermission: Permission.ManageAllEmails,
        tenantBoundPermission: null, // tenant bound has no requirements.
    },
    viewReports: {
        ownershipBoundPermission: null,
        organisationBoundPermission: null,
        tenantBoundPermission: Permission.ViewReports, // tenant bound has no requirements.
    },
    manageReports: {
        ownershipBoundPermission: null,
        organisationBoundPermission: null,
        tenantBoundPermission: Permission.ManageReports, // tenant bound has no requirements.
    },
};
