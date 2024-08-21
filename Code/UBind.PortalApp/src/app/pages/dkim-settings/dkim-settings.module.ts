import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared.module';
import { RouterModule } from '@angular/router';
import { PermissionGuard } from '../../providers/guard/permission.guard';
import { Permission } from '../../helpers/permissions.helper';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { DkimSettingsListPage } from './dkim-settings-list/dkim-settings-list.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { CreateEditDkimSettingPage } from './create-edit-dkim-settings/create-edit-dkim-settings.page';
import { ListOrganisationPage } from '../organisation/list-organisation/list-organisation.page';
import { DetailDkimSettingPage } from './detail-dkim-settings/detail-dkim-settings.page';
import { SendDkimTestEmailPage } from './dkim-setting-send-test-email/dkim-send-test-email.page';
import { FilterSortPage } from '../filter-sort/filter-sort-page';
import { ListOrganisationUserPage } from '../organisation/list-organisation-user/list-organisation-user.page';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
    },
    {
        path: 'list',
        component: DkimSettingsListPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'create',
        component: CreateEditDkimSettingPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
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
        path: ':dkimSettingsId/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'dkim-settings/list/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'dkim-settings/list/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: '/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationUserPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':dkimSettingsId',
        component: DetailDkimSettingPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':dkimSettingsId/send',
        component: SendDkimTestEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewOrganisations, Permission.ViewAllOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':dkimSettingsId/edit',
        component: CreateEditDkimSettingPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageOrganisations],
            masterComponent: ListOrganisationPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Module for DKIM settings.
 */
@NgModule({
    declarations: [
        DkimSettingsListPage,
        CreateEditDkimSettingPage,
        DetailDkimSettingPage,
        SendDkimTestEmailPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        RouterModule.forChild(routes),
    ],
})
export class DkimSettingsModule { }
