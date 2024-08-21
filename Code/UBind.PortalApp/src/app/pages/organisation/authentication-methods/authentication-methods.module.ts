import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { SharedComponentsModule } from "@app/components/shared-components.module";
import { Permission } from "@app/helpers/permissions.helper";
import { ListModule } from "@app/list.module";
import { MessageSharedComponentModule } from "@app/pages/message/message-shared-component.module";
import { PortalCommonModule } from "@app/pages/portal/portal-common.module";
import { UserSharedComponentsModule } from "@app/pages/user/user-shared-component.module";
import { BackNavigationGuard } from "@app/providers/guard/back-navigation.guard";
import { PermissionGuard } from "@app/providers/guard/permission.guard";
import { TypedRoutes } from "@app/routing/typed-route";
import { SharedModule } from "@app/shared.module";
import { ListOrganisationPage } from "../list-organisation/list-organisation.page";
import { LocalAccountAuthSettingsPage } from "./local-account-auth-settings/local-account-auth-settings.page";
import { SsoConfigurationsListPage } from "./sso-configurations-list/sso-configuration-list.page";
import { CreateEditSsoConfigurationPage } from "./create-edit-sso-configuration/create-edit-sso-configuration.page";
import { DetailSsoConfigurationPage } from "./detail-sso-configuration/detail-sso-configuration.page";
import { ShowSamlMetadataPage } from "./detail-sso-configuration/show-saml-metadata/show-saml-metadata.page";

const routes: TypedRoutes = [
    {
        path: 'local-account',
        component: LocalAccountAuthSettingsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageOrganisations, Permission.ManageAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'sso-configurations/list',
        component: SsoConfigurationsListPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageOrganisations, Permission.ManageAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'sso-configurations/create',
        component: CreateEditSsoConfigurationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageOrganisations, Permission.ManageAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'sso-configurations/:authenticationMethodId',
        component: DetailSsoConfigurationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageOrganisations, Permission.ManageAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'sso-configurations/:authenticationMethodId/metadata',
        component: ShowSamlMetadataPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageOrganisations, Permission.ManageAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'sso-configurations/:authenticationMethodId/edit',
        component: CreateEditSsoConfigurationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageOrganisations, Permission.ManageAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Specifies the module definition for the OrganisationAuthenticationMethods module.
 */
@NgModule({
    declarations: [
        LocalAccountAuthSettingsPage,
        SsoConfigurationsListPage,
        CreateEditSsoConfigurationPage,
        DetailSsoConfigurationPage,
        ShowSamlMetadataPage,
    ],
    imports: [
        PortalCommonModule,
        SharedModule,
        SharedComponentsModule,
        ListModule,
        UserSharedComponentsModule,
        MessageSharedComponentModule,
        CommonModule,
        RouterModule.forChild(routes),
    ],
})
export class OrganisationAuthenticationMethodsModule { }
