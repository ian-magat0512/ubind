import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { SharedComponentsModule } from "@app/components/shared-components.module";
import { ShowMasterComponentWhenNotSplit }
    from "@app/components/show-master-when-not-split/show-master-when-not-split.component";
import { Permission } from "@app/helpers";
import { ListModule } from "@app/list.module";
import { PermissionGuard } from "@app/providers/guard/permission.guard";
import { SharedModule } from "@app/shared.module";
import { ListNoSelectionPage } from "../list-no-selection/list-no-selection.page";
import { DetailOrganisationPage } from "./detail-organisation/detail-organisation.page";
import { ListOrganisationPage } from "./list-organisation/list-organisation.page";
import { PopoverOrganisationPage } from "./popover-organisation/popover-organisation.page";
import { CreateEditOrganisationPage } from "./create-edit-organisation/create-edit-organisation.page";
import { BackNavigationGuard } from "../../providers/guard/back-navigation.guard";
import { ListOrganisationUserPage } from "./list-organisation-user/list-organisation-user.page";
import { CreateUserPage } from "../user/create-user/create-user.page";
import { OrganisationUserViewComponent } from "./organisation-user-view/organisation-user-view.component";
import { UploadPictureUserPage } from "../user/upload-picture-user/upload-picture-user.page";
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';
import { DetailEmailPage } from "@pages/message/detail-email/detail-email.page";
import { AssignRolePage } from "@pages/user/assign-role/assign-role.page";
import { DetailUserPage } from "@pages/user/detail-user/detail-user.page";
import { OrganisationPortalViewComponent } from "./organisation-portal-view/organisation-portal-view.component";
import { PortalCommonModule } from "@pages/portal/portal-common.module";
import { UserSharedComponentsModule } from "@pages/user/user-shared-component.module";
import { MessageSharedComponentModule } from "@pages/message/message-shared-component.module";
import { DetailSmsPage } from "@pages/message/detail-sms/detail-sms.page";
import { ListOrganisationUserMessagePage } from "./list-organisation-user-message/list-organisation-user-message.page";
import { DataTableSharedComponentModule } from "@pages/data-table/data-table-shared-component.module";
import { ListDataTableDefinitionPage } from "../data-table/list-data-table-definition/list-data-table-definition.page";
import { DataTableModule } from "../data-table/data-table.module";
import { AdditionalPropertyDefinitionModule }
    from "../additional-property-definition/additional-property-definition.module";
import { DkimSettingsModule } from "../dkim-settings/dkim-settings.module";
import { TypedRoutes } from "@app/routing/typed-route";
import { EditUserPage } from "../user/edit-user/edit-user.page";
import { AssignManagingOrganisationPage } from "./assign-managing-organisation/assign-managing-organisation.page";
import { OrganisationAuthenticationMethodsModule } from "./authentication-methods/authentication-methods.module";
import { PortalModule } from "@pages/portal/portal.module";
import { AssignUserPortalComponent } from '@pages/user/assign-user-portal/assign-user-portal.component';

const routes: TypedRoutes = [
    {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
    },
    {
        path: 'list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select an organisation to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateEditOrganisationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':organisationId/edit',
        component: CreateEditOrganisationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    }, {
        path: ':organisationId/managing-organisation',
        component: AssignManagingOrganisationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    }, {
        path: ':organisationId/user/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfEachSetOfPermissions: [
                [Permission.ViewOrganisations, Permission.ManageOrganisations],
                [
                    Permission.ViewUsers,
                    Permission.ManageUsers,
                    Permission.ViewUsersFromOtherOrganisations,
                    Permission.ManageUsersForOtherOrganisations,
                ],
            ],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':organisationId/user/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfEachSetOfPermissions: [
                [Permission.ViewOrganisations, Permission.ManageOrganisations],
                [
                    Permission.ViewUsers,
                    Permission.ManageUsers,
                    Permission.ViewUsersFromOtherOrganisations,
                    Permission.ManageUsersForOtherOrganisations,
                ],
            ],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a user to view details',
        },
    },
    {
        path: ':organisationId/user/create',
        component: CreateUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':organisationId/user/:userId',
        component: DetailUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfEachSetOfPermissions: [
                [Permission.ViewOrganisations, Permission.ManageOrganisations],
                [
                    Permission.ViewUsers,
                    Permission.ManageUsers,
                    Permission.ViewUsersFromOtherOrganisations,
                    Permission.ManageUsersForOtherOrganisations,
                ],
            ],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':organisationId/user/:userId/edit',
        component: EditUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':organisationId/user/:userId/picture/upload',
        component: UploadPictureUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    }, {
        path: ':organisationId/user/:userId/message/email/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListOrganisationUserMessagePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a message to view details',
        },
    },
    {
        path: ':organisationId/user/:userId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListOrganisationUserMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':organisationId/user/:userId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListOrganisationUserMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':organisationId/user/:userId/role/assign',
        component: AssignRolePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':organisationId/user/:userId/assign-portal',
        component: AssignUserPortalComponent,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfEachSetOfPermissions: [
                [Permission.ViewOrganisations, Permission.ManageOrganisations],
                [
                    Permission.ViewUsers,
                    Permission.ManageUsers,
                    Permission.ViewUsersFromOtherOrganisations,
                    Permission.ManageUsersForOtherOrganisations,
                ],
            ],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ":organisationId/portal",
        loadChildren: () => PortalModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPortals],
        },
    },
    {
        path: ':organisationId',
        component: DetailOrganisationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':contextId/additional-property-definition',
        loadChildren: () => AdditionalPropertyDefinitionModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewAdditionalPropertyValues],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':organisationId/dkim-settings',
        loadChildren: () => DkimSettingsModule,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':organisationId/authentication-method',
        loadChildren: () => OrganisationAuthenticationMethodsModule,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':organisationId/data-table',
        loadChildren: () => DataTableModule,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfEachSetOfPermissions: [
                [Permission.ViewOrganisations, Permission.ViewAllOrganisations, Permission.ManageOrganisations],
                [Permission.ViewDataTables, Permission.ManageDataTables],
            ],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':organisationId/data-table/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewDataTables],
            masterComponent: ListDataTableDefinitionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
];

/**
 * Specifies the module definition for the Organisation module.
 */
@NgModule({
    declarations: [
        ListOrganisationPage,
        DetailOrganisationPage,
        PopoverOrganisationPage,
        CreateEditOrganisationPage,
        OrganisationUserViewComponent,
        OrganisationPortalViewComponent,
        ListOrganisationUserPage,
        ListOrganisationUserMessagePage,
        AssignManagingOrganisationPage,
    ],
    imports: [
        PortalCommonModule,
        SharedModule,
        SharedComponentsModule,
        ListModule,
        UserSharedComponentsModule,
        MessageSharedComponentModule,
        DataTableSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class OrganisationModule { }
