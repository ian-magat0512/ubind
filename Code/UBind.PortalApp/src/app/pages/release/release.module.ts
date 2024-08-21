import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { CreateEditReleasePage } from './create-edit-release/create-edit-release.page';
import { DetailReleasePage } from './detail-release/detail-release.page';
import { ListReleasePage } from './list-release/list-release.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers';
import { ListModule } from '@app/list.module';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';
import { TypedRoutes } from '@app/routing/typed-route';
import { SelectProductRelease } from './product-release/select-product-release.page';

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
            mustHavePermissions: [Permission.ViewReleases],
            masterComponent: ListReleasePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a release to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewReleases],
            masterComponent: ListReleasePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateEditReleasePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageReleases],
            masterComponent: ListReleasePage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':releaseId',
        component: DetailReleasePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewReleases],
            masterComponent: ListReleasePage,
            masterContainerClass: 'master-list',
        },
    },
    {
        path: ':releaseId/edit',
        component: CreateEditReleasePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewReleases],
            masterComponent: ListReleasePage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':releaseId/select-product-release/:environment',
        component: SelectProductRelease,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageReleases],
            masterComponent: ListReleasePage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export Release module class.
 * This class manage Ng Module declaration of release.
 */
@NgModule({
    declarations: [
        CreateEditReleasePage,
        DetailReleasePage,
        ListReleasePage,
        SelectProductRelease,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        RouterModule.forChild(routes),
    ],
})
export class ReleaseModule { }
