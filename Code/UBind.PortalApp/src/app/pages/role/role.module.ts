import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { Permission } from '@app/helpers/permissions.helper';
import { ListModule } from '@app/list.module';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { SharedModule } from '@app/shared.module';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { CreateEditRolePermissionPage } from './create-edit-role-permission/create-edit-role-permission.page';
import { CreateEditRolePage } from './create-edit-role/create-edit-role.page';
import { DetailRolePermissionPage } from './detail-role-permission/detail-role-permission.page';
import { DetailRolePage } from './detail-role/detail-role.page';
import { ListRolePage } from './list-role/list-role.page';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';

const roleRoutes: Routes = [
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
            mustHavePermissions: [Permission.ViewRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a role to view details.',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateEditRolePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':roleId',
        component: DetailRolePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':roleId/segment/:segment',
        component: DetailRolePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':roleId/edit',
        component: CreateEditRolePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':roleId/permission',
        component: CreateEditRolePermissionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':roleId/permission/:permissionType',
        component: DetailRolePermissionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':roleId/permission/:permissionType/edit',
        component: CreateEditRolePermissionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageRoles],
            masterComponent: ListRolePage,
            masterContainerClass: 'master-list',
        },
    },
];

/**
 * Export role module class.
 * This class manage Ng Module declarations of role.
 */
@NgModule({
    declarations: [
        ListRolePage,
        CreateEditRolePage,
        DetailRolePage,
        CreateEditRolePermissionPage,
        DetailRolePermissionPage,
    ],
    imports: [
        ReactiveFormsModule,
        SharedModule,
        SharedComponentsModule,
        ListModule,
        RouterModule.forChild(roleRoutes),
    ],
})
export class RoleModule { }
