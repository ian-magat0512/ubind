import { Injectable } from "@angular/core";
import { DeploymentEnvironment } from "@app/models/deployment-environment.enum";
import {
    Permission,
    PermissionDataModel,
    ElevatedPermission,
} from "@app/helpers/permissions.helper";
import { AuthenticationService } from "./authentication.service";
import { FeatureSettingService } from "./feature-setting.service";
import { FeatureSetting } from "@app/models/feature-setting.enum";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { StringHelper } from "@app/helpers";

/**
 * Export permission service class.
 * TODO: Write a better class header: permissions of the users.
 */
@Injectable({ providedIn: "root" })
export class PermissionService {

    // easily identify other elevated permissions of a permission.
    private elevatedPermissionsMapping: { [key: string]: ElevatedPermission } = {
        manageUsers: {
            ownershipBoundPermission: null,
            organisationBoundPermission: Permission.ManageUsers,
            tenantBoundPermission: Permission.ManageUsersForOtherOrganisations,
            requiresFeature: FeatureSetting.UserManagement,
        },
        manageQuotes: {
            ownershipBoundPermission: Permission.ManageQuotes,
            organisationBoundPermission: Permission.ManageAllQuotes,
            tenantBoundPermission: Permission.ManageAllQuotesForAllOrganisations,
            requiresFeature: FeatureSetting.QuoteManagement,
        },
        managePolicies: {
            ownershipBoundPermission: Permission.ManagePolicies,
            organisationBoundPermission: Permission.ManageAllPolicies,
            tenantBoundPermission: Permission.ManageAllPoliciesForAllOrganisations,
            requiresFeature: FeatureSetting.PolicyManagement,
        },
        manageCustomers: {
            ownershipBoundPermission: Permission.ManageCustomers,
            organisationBoundPermission: Permission.ManageAllCustomers,
            tenantBoundPermission: Permission.ManageAllCustomersForAllOrganisations,
            requiresFeature: FeatureSetting.CustomerManagement,
        },
        manageClaims: {
            ownershipBoundPermission: Permission.ManageClaims,
            organisationBoundPermission: Permission.ManageAllClaims,
            tenantBoundPermission: Permission.ManageAllClaimsForAllOrganisations,
            requiresFeature: FeatureSetting.ClaimsManagement,
        },
        manageRoles: {
            ownershipBoundPermission: null,
            organisationBoundPermission: Permission.ManageRoles,
            tenantBoundPermission: Permission.ManageRolesForAllOrganisations,
            requiresFeature: null, // null means t here is no required feature
        },
        viewUsers: {
            ownershipBoundPermission: null,
            organisationBoundPermission: Permission.ViewUsers,
            tenantBoundPermission: Permission.ViewUsersFromOtherOrganisations,
            requiresFeature: FeatureSetting.UserManagement,
        },
        viewQuotes: {
            ownershipBoundPermission: Permission.ViewQuotes,
            organisationBoundPermission: Permission.ViewAllQuotes,
            tenantBoundPermission: Permission.ViewAllQuotesFromAllOrganisations,
            requiresFeature: FeatureSetting.QuoteManagement,
        },
        viewPolicies: {
            ownershipBoundPermission: Permission.ViewPolicies,
            organisationBoundPermission: Permission.ViewAllPolicies,
            tenantBoundPermission: Permission.ViewAllPoliciesFromAllOrganisations,
            requiresFeature: FeatureSetting.PolicyManagement,
        },
        viewCustomers: {
            ownershipBoundPermission: Permission.ViewCustomers,
            organisationBoundPermission: Permission.ViewAllCustomers,
            tenantBoundPermission: Permission.ViewAllCustomersFromAllOrganisations,
            requiresFeature: FeatureSetting.CustomerManagement,
        },
        viewClaims: {
            ownershipBoundPermission: Permission.ViewClaims,
            organisationBoundPermission: Permission.ViewAllClaims,
            tenantBoundPermission: Permission.ViewAllClaimsFromAllOrganisations,
            requiresFeature: FeatureSetting.ClaimsManagement,
        },
        viewRoles: {
            ownershipBoundPermission: null,
            organisationBoundPermission: Permission.ViewRoles,
            tenantBoundPermission: Permission.ViewRolesFromAllOrganisations,
            requiresFeature: FeatureSetting.UserManagement, // null means there is no required feature
        },
        manageOrganisations: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ManageOrganisations,
            requiresFeature: FeatureSetting.OrganisationManagement,
        },
        viewOrganisations: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ViewOrganisations,
            requiresFeature: FeatureSetting.OrganisationManagement,
        },
        viewPortals: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ViewPortals,
            requiresFeature: FeatureSetting.PortalManagement,
        },
        managePortals: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ManagePortals,
            requiresFeature: FeatureSetting.PortalManagement,
        },
        viewMessages: {
            ownershipBoundPermission: Permission.ViewMessages,
            organisationBoundPermission: Permission.ViewAllMessages,
            tenantBoundPermission: Permission.ViewAllCustomersFromAllOrganisations,
            requiresFeature: FeatureSetting.MessageManagement,
        },
        manageMessages: {
            ownershipBoundPermission: Permission.ManageMessages,
            organisationBoundPermission: Permission.ManageAllEmails,
            tenantBoundPermission: null, // tenant bound has no requirements.
            requiresFeature: FeatureSetting.MessageManagement,
        },
        viewReports: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ViewReports, // tenant bound has no requirements.
            requiresFeature: FeatureSetting.ReportManagement,
        },
        manageReports: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ManageReports, // tenant bound has no requirements.
            requiresFeature: FeatureSetting.ReportManagement,
        },
        viewProducts: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ViewProducts, // tenant bound has no requirements.
            requiresFeature: FeatureSetting.ProductManagement,
        },
        manageProducts: {
            ownershipBoundPermission: null,
            organisationBoundPermission: null,
            tenantBoundPermission: Permission.ManageProducts, // tenant bound has no requirements.
            requiresFeature: FeatureSetting.ProductManagement,
        },
    };

    private relatedEntityToPermissionMapping: { [key: string]: Permission } = {
        claim: Permission.ViewClaims,
        customer: Permission.ViewCustomers,
        policy: Permission.ViewPolicies,
        policyTransaction: Permission.ViewPolicies,
        quote: Permission.ViewQuotes,
        organisation: Permission.ViewOrganisations,
        user: Permission.ViewUsers,
        portal: Permission.ViewPortals,
        product: Permission.ViewProducts,
        release: Permission.ViewReleases,
    };

    public constructor(
        private authenticationService: AuthenticationService,
        private featureSettingService: FeatureSettingService,
    ) { }

    public getPermissions(): Array<Permission> {
        let permissions: Array<Permission> = this.authenticationService.permissions;
        return permissions;
    }

    /**
     * @returns true if the user has any of the permissions listed.
     */
    public hasOneOfPermissions(permissions: Array<Permission>): boolean {
        if (!permissions) {
            return false;
        }

        for (const permission of permissions) {
            if (this.hasPermission(permission, true)) {
                return true;
            }
        }

        return false;
    }

    /**
     * @returns true if the user has at least one permission in each set of permissions listed.
     */
    public hasOneOfEachSetOfPermissions(permissionSets: Array<Array<Permission>>): boolean {
        if (!permissionSets) {
            return false;
        }

        for (const permissionSet of permissionSets) {
            if (!this.hasOneOfPermissions(permissionSet)) {
                return false;
            }
        }

        return true;
    }

    public hasPermission(permission: Permission, includeAllRelatedPermissions: boolean = true): boolean {
        if (!permission) {
            return false;
        }

        let userPermissions: Array<Permission> = this.getPermissions();
        if (userPermissions == null) {
            return false;
        }

        // check elevated privileges.
        let elevatedPermissions: ElevatedPermission = this.retrieveRelatedPermissions(permission);
        let portalFeatureIsLoaded: boolean = this.featureSettingService.getFeatures().length > 0;
        if (elevatedPermissions) {
            let hasFeature: boolean =
                elevatedPermissions.requiresFeature
                    ? this.featureSettingService.hasFeature(elevatedPermissions.requiresFeature)
                    : false;
            let allow: boolean =
                !elevatedPermissions.requiresFeature
                || !portalFeatureIsLoaded
                || (portalFeatureIsLoaded && hasFeature);
            if (includeAllRelatedPermissions) {
                return (userPermissions.indexOf(elevatedPermissions.tenantBoundPermission) > -1
                    || userPermissions.indexOf(elevatedPermissions.organisationBoundPermission) > -1
                    || userPermissions.indexOf(elevatedPermissions.ownershipBoundPermission) > -1)
                    && allow;
            } else {
                return (userPermissions.indexOf(permission) > -1) && allow;
            }
        } else {
            return userPermissions.indexOf(permission) > -1;
        }
    }

    // checks elevated permissions regarding the passed relatedEntityType
    public hasElevatedPermissionsOfTheRelatedEntity(
        relatedEntityType: RelatedEntityType,
        model: PermissionDataModel,
    ): boolean {
        let camelCaseEntityType: string = StringHelper.camelCase(relatedEntityType);
        let permission: Permission = this.relatedEntityToPermissionMapping[camelCaseEntityType];
        if (!permission) {
            return false;
        }

        return this.hasElevatedPermissionsViaModel(
            permission,
            model,
        );
    }

    public hasElevatedPermissionsViaModel(
        permission: Permission,
        model: PermissionDataModel,
    ): boolean {
        return this.hasElevatedPermissions(
            permission,
            model.organisationId,
            model.ownerUserId,
            model.customerId,
        );
    }

    public hasElevatedPermissions(
        permission: Permission,
        organisationId: string,
        ownerUserId: string,
        customerId: string,
    ): boolean {
        if (!permission) {
            return false;
        }

        let elevatedPermissions: ElevatedPermission =
            this.retrieveRelatedPermissions(permission);

        if (!elevatedPermissions) {
            return this.hasPermission(permission);
        }

        if (!elevatedPermissions.requiresFeature
            || (elevatedPermissions.requiresFeature
                ? this.featureSettingService.hasFeature(elevatedPermissions.requiresFeature)
                : false)) {
            if (elevatedPermissions.tenantBoundPermission
                && this.hasPermission(elevatedPermissions.tenantBoundPermission, false)) {
                return true;
            } else if (elevatedPermissions.organisationBoundPermission
                && this.hasPermission(elevatedPermissions.organisationBoundPermission, false)
                && this.authenticationService.userOrganisationId == organisationId) {
                return true;
            } else if (elevatedPermissions.ownershipBoundPermission
                && this.hasPermission(elevatedPermissions.ownershipBoundPermission, false)
                && (this.authenticationService.userId == ownerUserId
                    || this.authenticationService.customerId == customerId)) {
                return true;
            }
        }

        return false;
    }

    public getAvailableEnvironments(): Array<DeploymentEnvironment> {
        let environments: Array<DeploymentEnvironment> = new Array<DeploymentEnvironment>();
        if (this.hasPermission(Permission.AccessDevelopmentData)) {
            environments.push(DeploymentEnvironment.Development);
        }
        if (this.hasPermission(Permission.AccessStagingData)) {
            environments.push(DeploymentEnvironment.Staging);
        }
        if (this.hasPermission(Permission.AccessProductionData)) {
            environments.push(DeploymentEnvironment.Production);
        }
        return environments;
    }

    public canAccessEnvironment(environment: DeploymentEnvironment): boolean {
        return this.getAvailableEnvironments()
            .findIndex((e: DeploymentEnvironment) => e.toLowerCase() == environment.toLowerCase()) > -1;
    }

    public hasViewPortal(): boolean {
        return this.hasPermission(Permission.ViewPortals)
            && this.featureSettingService.hasFeature(FeatureSetting.PortalManagement);
    }

    public hasManageClaimPermission(): boolean {
        return (this.hasPermission(Permission.ManageClaims)
            || this.hasPermission(Permission.ManageAllClaims)
            || this.hasPermission(Permission.ManageAllClaimsForAllOrganisations))
            && this.featureSettingService.hasClaimFeature();
    }

    public hasManageQuotePermission(): boolean {
        return (this.hasPermission(Permission.ManageQuotes)
            || this.hasPermission(Permission.ManageAllQuotes)
            || this.hasPermission(Permission.ManageAllQuotesForAllOrganisations))
            && this.featureSettingService.hasQuoteFeature();
    }

    public hasManageUserPermission(): boolean {
        return (this.hasPermission(Permission.ManageUsers)
            || this.hasPermission(Permission.ManageUsersForOtherOrganisations))
            && this.featureSettingService.hasUserManagementFeature();
    }

    public hasManageProductPermission(): boolean {
        return (this.hasPermission(Permission.ManageProducts))
            && this.featureSettingService.hasProductFeature();
    }

    public hasViewPolicyPermission(): boolean {
        return (this.hasPermission(Permission.ViewPolicies)
            || this.hasPermission(Permission.ViewAllPolicies)
            || this.hasPermission(Permission.ViewAllPoliciesFromAllOrganisations))
            && this.featureSettingService.hasPolicyFeature();
    }

    public hasViewOrganisationPermission(): boolean {
        return (this.hasPermission(Permission.ViewOrganisations)
            || this.hasPermission(Permission.ViewAllOrganisations)
            || this.hasPermission(Permission.ManageOrganisations)
            || this.hasPermission(Permission.ManageAllOrganisations))
            && this.featureSettingService.hasOrganisationFeature();
    }

    public hasViewEmailPermission(): boolean {
        return (this.hasPermission(Permission.ViewMessages)
            || this.hasPermission(Permission.ViewAllMessages))
            && this.featureSettingService.hasEmailFeature();
    }

    public hasViewClaimPermission(): boolean {
        return (this.hasPermission(Permission.ViewClaims)
            || this.hasPermission(Permission.ViewAllClaims)
            || this.hasPermission(Permission.ViewAllClaimsFromAllOrganisations))
            && this.featureSettingService.hasClaimFeature();
    }

    public hasViewQuotePermission(): boolean {
        return (this.hasPermission(Permission.ViewQuotes)
            || this.hasPermission(Permission.ViewAllQuotes)
            || this.hasPermission(Permission.ViewAllQuotesFromAllOrganisations))
            && this.featureSettingService.hasQuoteFeature();
    }

    public hasViewCustomerPermission(): boolean {
        return (this.hasPermission(Permission.ViewCustomers)
            || this.hasPermission(Permission.ViewAllCustomers)
            || this.hasPermission(Permission.ViewAllCustomersFromAllOrganisations))
            && this.featureSettingService.hasCustomerFeature();
    }

    public hasViewUserPermission(): boolean {
        return (this.hasPermission(Permission.ViewUsers)
            || this.hasPermission(Permission.ViewUsersFromOtherOrganisations))
            && this.featureSettingService.hasUserManagementFeature();
    }

    public canAccessOtherEnvironments(): boolean {
        let numberOfEnvironments: number = 0;
        numberOfEnvironments += this.hasPermission(Permission.AccessDevelopmentData) ? 1 : 0;
        numberOfEnvironments += this.hasPermission(Permission.AccessStagingData) ? 1 : 0;
        numberOfEnvironments += this.hasPermission(Permission.AccessProductionData) ? 1 : 0;
        return numberOfEnvironments > 1;
    }

    public retrieveRelatedPermissions(permission: Permission): ElevatedPermission {
        if (!permission) {
            return null;
        }

        for (let prop in this.elevatedPermissionsMapping) {
            let elevatedPermission: ElevatedPermission = this.elevatedPermissionsMapping[prop];

            if (elevatedPermission.tenantBoundPermission == permission
                || elevatedPermission.organisationBoundPermission == permission
                || elevatedPermission.ownershipBoundPermission == permission) {
                return elevatedPermission;
            }
        }

        return null;
    }
}
