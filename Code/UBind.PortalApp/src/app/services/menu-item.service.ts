import { Injectable } from "@angular/core";
import { Permission } from "@app/helpers/permissions.helper";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { Errors } from "@app/models/errors";
import { FeatureSetting } from "@app/models/feature-setting.enum";
import { MenuItem } from "@app/models/menu-item";
import { PortalPageWidget } from "@app/models/portal-page-widget.enum";
import { UserType } from "@app/models/user-type.enum";
import { AuthenticationService } from "@app/services/authentication.service";
import { FeatureSettingService } from "@app/services/feature-setting.service";
import { PermissionService } from "@app/services/permission.service";
import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Provides the menu items for different user types
 */
@Injectable({ providedIn: 'root' })
export class MenuItemService {

    public constructor(
        private userPath: UserTypePathHelper,
        private authenticationService: AuthenticationService,
        private featureSettingService: FeatureSettingService,
        private permissionService: PermissionService,
    ) { }

    public getMenuItems(): Array<MenuItem> {
        const userType: UserType = this.authenticationService.userType;
        const menuItems: Array<MenuItem>
            = this.featureSettingService.removeMenuItemsForDisabledFeatures(this.getMenuItemsForUserType(userType));
        if (menuItems.length == 0) {
            return this.getHomePage(menuItems);
        } else {
            return menuItems;
        }
    }

    private getHomePage(menuItems: Array<MenuItem>): Array<MenuItem> {
        return menuItems.filter((page: MenuItem) => page.identifier === PortalPageWidget.Home);
    }

    private getMenuItemsForUserType(userType: UserType): Array<MenuItem> {
        switch (userType) {
            case UserType.Client:
                return this.getClientMenuItems();
            case UserType.Master:
                return this.getMasterMenuItems();
            case UserType.Customer:
                return this.getCustomerMenuItems();
            default:
                throw Errors.General.Unexpected(`The user type "${userType}"" is not one of the known user types.`);
        }
    }

    private getClientMenuItems(): Array<MenuItem> {
        const clientAdminMenuItems: Array<MenuItem> = [
            {
                identifier: "Home",
                title: "Home",
                icon: "ios-home",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['home'] },
            },
            {
                identifier: "Customers",
                requiresFeature: FeatureSetting.CustomerManagement,
                title: "Customers",
                icon: "people",
                iconLibrary: IconLibrary.IonicV5,
                navigate: { commands: ['customer', 'list'] },
                permissions: [Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ManageCustomers],
            },
            {
                identifier: "Quotes",
                requiresFeature: FeatureSetting.QuoteManagement,
                title: "Quotes",
                icon: "calculator",
                iconLibrary: IconLibrary.AngularMaterial,
                navigate: { commands: [this.userPath.quote, 'list'] },
                permissions:
                    [Permission.ViewQuotes,
                        Permission.ViewAllQuotes,
                        Permission.ViewAllQuotesFromAllOrganisations],
            },
            {
                identifier: "Policies",
                requiresFeature: FeatureSetting.PolicyManagement,
                title: this.authenticationService.isMutualTenant() ? "Protections" : "Policies",
                icon: "shield",
                iconLibrary: IconLibrary.AngularMaterial,
                navigate: { commands: [this.userPath.policy, 'list'] },
                permissions:
                    [Permission.ViewPolicies,
                        Permission.ViewAllPolicies,
                        Permission.ViewAllPoliciesFromAllOrganisations],
            },
            {
                identifier: "Claims",
                requiresFeature: FeatureSetting.ClaimsManagement,
                title: "Claims",
                icon: "clipboard",
                iconLibrary: IconLibrary.AngularMaterial,
                navigate: { commands: [this.userPath.claim, 'list'] },
                permissions:
                    [Permission.ViewClaims,
                        Permission.ViewAllClaims,
                        Permission.ViewAllClaimsFromAllOrganisations],
            },
            {
                identifier: "Messages",
                requiresFeature: FeatureSetting.MessageManagement,
                title: "Messages",
                icon: "chatboxes",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: [this.userPath.message, 'list'] },
                permissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            },
            {
                identifier: 'Reports',
                requiresFeature: FeatureSetting.ReportManagement,
                title: 'Reports',
                icon: 'pie',
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['report', 'list'] },
                permissions: [Permission.ViewReports, Permission.ManageReports],
            },
            {
                identifier: "Organisations",
                requiresFeature: FeatureSetting.OrganisationManagement,
                title: "Organisations",
                icon: "business",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['organisation', 'list'] },
                permissions: [Permission.ViewOrganisations, Permission.ManageOrganisations],
            },
            {
                identifier: "Users",
                requiresFeature: FeatureSetting.UserManagement,
                title: "Users",
                icon: "contact",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['user', 'list'] },
                permissions: [Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations],
            },
            {
                identifier: "Roles",
                requiresFeature: FeatureSetting.UserManagement,
                title: "Roles",
                icon: "shirt",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['role', 'list'] },
                permissions: [Permission.ViewRoles, Permission.ViewRolesFromAllOrganisations],
            },
            {
                identifier: "Portals",
                requiresFeature: FeatureSetting.PortalManagement,
                title: "Portals",
                icon: "browsers",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['portal', 'list'] },
                permissions: [Permission.ViewPortals, Permission.ManagePortals],
            },
            {
                identifier: "Products",
                requiresFeature: FeatureSetting.ProductManagement,
                title: "Products",
                icon: "cube",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['product', 'list'] },
                permissions: [Permission.ViewProducts, Permission.ManageProducts],
            },
        ].filter((p: MenuItem) =>
            p.identifier === "Home"
            ||
            (this.permissionService.hasOneOfPermissions(p.permissions)
                && (p.requiresFeature
                    ? this.featureSettingService.hasFeature(p.requiresFeature)
                    : false)));
        return clientAdminMenuItems;
    }

    private getMasterMenuItems(): Array<MenuItem> {
        const masterAdminMenuItems: Array<MenuItem> = [
            {
                identifier: "Home",
                title: "Home",
                icon: "ios-home",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: [this.userPath.home] },
            },
            {
                identifier: "Tenants",
                title: "Tenants",
                icon: "cloud-circle",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['tenant', 'list'] },
                permissions: [Permission.ViewTenants],
            },
            {
                identifier: "Messages",
                title: "Messages",
                icon: "chatboxes",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: [this.userPath.message, 'list'] },
                permissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            },
            {
                identifier: "Organisations",
                title: "Organisations",
                icon: "business",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['organisation', 'list'] },
                permissions: [Permission.ViewOrganisations],
            },
            {
                identifier: "Users",
                title: "Users",
                icon: "contact",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['user', 'list'] },
                permissions: [Permission.ViewUsers],
            },
            {
                identifier: "Roles",
                title: "Roles",
                icon: "shirt",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['role', 'list'] },
                permissions: [Permission.ViewRoles],
            },
        ].filter((p: MenuItem) =>
            p.identifier === "Home" ? true : this.permissionService.hasOneOfPermissions(p.permissions));

        return masterAdminMenuItems;
    }

    private getCustomerMenuItems(): Array<MenuItem> {
        const customerMenuItems: Array<MenuItem> = [
            {
                identifier: "Home",
                title: "Home",
                icon: "ios-home",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: [this.userPath.home] },
            },
            {
                identifier: "Claims",
                requiresFeature: FeatureSetting.ClaimsManagement,
                title: "My Claims",
                icon: "clipboard",
                iconLibrary: IconLibrary.AngularMaterial,
                navigate: { commands: [this.userPath.claim, 'list'] },
                permissions: [Permission.ViewClaims],
            },
            {
                identifier: "Policies",
                requiresFeature: FeatureSetting.PolicyManagement,
                title: this.authenticationService.isMutualTenant() ? "My Protections" : "My Policies",
                icon: "shield",
                iconLibrary: IconLibrary.AngularMaterial,
                navigate: { commands: [this.userPath.policy, 'list'] },
                permissions: [Permission.ViewPolicies],
            },
            {
                identifier: "Quotes",
                requiresFeature: FeatureSetting.QuoteManagement,
                title: "My Quotes",
                icon: "calculator",
                iconLibrary: IconLibrary.AngularMaterial,
                navigate: { commands: [this.userPath.quote, 'list'] },
                permissions: [Permission.ViewQuotes],
            },
            {
                identifier: "Messages",
                requiresFeature: FeatureSetting.MessageManagement,
                title: "My Messages",
                icon: "chatboxes",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: [this.userPath.message, 'list'] },
                permissions: [Permission.ViewMessages],
            },
        ].filter((p: MenuItem) =>
            p.identifier === "Home"
            ||
            (this.permissionService.hasOneOfPermissions(p.permissions)
                && (p.requiresFeature
                    ? this.featureSettingService.hasFeature(p.requiresFeature)
                    : false)));

        return customerMenuItems;
    }
}
