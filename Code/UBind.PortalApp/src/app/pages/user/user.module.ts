import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { CreateUserPage } from './create-user/create-user.page';
import { DetailUserPage } from './detail-user/detail-user.page';
import { EditUserPage } from './edit-user/edit-user.page';
import { ListUserPage } from './list-user/list-user.page';
import { UploadPictureUserPage } from './upload-picture-user/upload-picture-user.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { ListModule } from '@app/list.module';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { AssignRolePage } from './assign-role/assign-role.page';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';
import { ListUserMessagePage } from './list-user-message/list-user-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { UserSharedComponentsModule } from './user-shared-component.module';
import { TypedRoutes } from '@app/routing/typed-route';
import { AssignUserPortalComponent } from './assign-user-portal/assign-user-portal.component';

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
            mustHaveOneOfPermissions: [Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a user to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':userId/edit',
        component: EditUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':userId/picture/upload',
        component: UploadPictureUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':userId/role/assign',
        component: AssignRolePage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':userId',
        component: DetailUserPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':userId/assign-portal',
        component: AssignUserPortalComponent,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageUsers],
            masterComponent: ListUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':userId/message/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListUserMessagePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a message to view details',
        },
    },
    {
        path: ':userId/message/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListUserMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':userId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListUserMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':userId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListUserMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
];

/**
 * Export User module class.
 * This class manage Ng Module declaration of user.
 */
@NgModule({
    declarations: [
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        UserSharedComponentsModule,
        MessageSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class UserModule { }
