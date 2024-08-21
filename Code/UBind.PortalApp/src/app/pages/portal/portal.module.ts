import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ListPortalPage } from '@app/pages/portal/list-portal/list-portal.page';
import { DetailPortalPage } from '@app/pages/portal/detail-portal/detail-portal.page';
import { CreateEditPortalPage } from './create-edit-portal/create-edit-portal.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { FilterSortPage } from '../filter-sort/filter-sort-page';
import { PortalCommonModule } from './portal-common.module';
import { PageRouteIndentifier } from '@app/helpers/page-route-identifier.helper';
import { EditPortalLocationPage } from './edit-portal-location/edit-portal-location.page';
import { EditPortalThemePage } from './edit-portal-theme/edit-portal-theme.page';
import { TypedRoutes } from '@app/routing/typed-route';
import { EditEmailTemplateModule } from '../edit-email-template/edit-email-template.module';
import { ManagePortalSignInMethodsPage } from './sign-in-methods/manage-portal-sign-in-methods.page';

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
            mustHaveOneOfPermissions: [Permission.ViewPortals, Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a portal to view details',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewPortals, Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
    },
    {
        path: 'create',
        component: CreateEditPortalPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':portalId',
        component: DetailPortalPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewPortals, Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
    },
    {
        path: ':portalId/edit',
        component: CreateEditPortalPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':portalId/theme',
        component: EditPortalThemePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':portalId/location/:environment',
        component: EditPortalLocationPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewPortals, Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':portalId/sign-in-methods',
        component: ManagePortalSignInMethodsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':portalId/email-template',
        loadChildren: () => EditEmailTemplateModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManagePortals],
            masterComponent: ListPortalPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
            routeIdentifier: PageRouteIndentifier.PortalListPage,
        },
    },
];

/**
 * Export portal module class.
 * This class manage Ng Module portal.
 */
@NgModule({
    declarations: [],
    imports: [
        PortalCommonModule,
        RouterModule.forChild(routes),
    ],
})
export class PortalModule { }
