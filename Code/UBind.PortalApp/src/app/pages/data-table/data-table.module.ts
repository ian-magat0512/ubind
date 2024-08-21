import { NgModule } from '@angular/core';
import {
    DataTableDefinitionListDetailPage,
} from './data-table-definition-list-detail/data-table-definition-list-detail.page';
import { RouterModule } from '@angular/router';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { SharedModule } from '@app/shared.module';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { CreateDataTablePage } from './create-data-table/create-data-table.page';
import { DetailDataTableDefinitionPage } from './detail-data-table-definition/detail-data-table-definition.page';
import { EditDataTablePage } from './edit-data-table/edit-data-table.page';
import { Permission } from '@app/helpers';
import { ListModule } from '@app/list.module';
import { ListDataTableDefinitionPage } from './list-data-table-definition/list-data-table-definition.page';
import { FilterSortPage } from '../filter-sort/filter-sort-page';
import {
    ShowMasterComponentWhenNotSplit,
} from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { DataTableSharedComponentModule } from './data-table-shared-component.module';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
    },
    {
        path: 'list-detail',
        component: DataTableDefinitionListDetailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewDataTables],
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewDataTables],
            masterComponent: ListDataTableDefinitionPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a data table to view details',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'create',
        component: CreateDataTablePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageDataTables],
            masterComponent: ListDataTableDefinitionPage,
            masterContainerClass: 'master-list',
            noSelectionMessage: 'Select a data table definition to view details',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: 'filter',
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
    {
        path: ':dataTableDefinitionId',
        component: DetailDataTableDefinitionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewDataTables],
            masterComponent: ListDataTableDefinitionPage,
            masterContainerClass: 'master-list',
            noSelectionMessage: 'Select a data table definition to view details',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':dataTableDefinitionId/edit',
        component: EditDataTablePage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageDataTables],
            masterComponent: ListDataTableDefinitionPage,
            masterContainerClass: 'master-list',
            noSelectionMessage: 'Select a data table definition to view details',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Data table module.
 */
@NgModule({
    declarations: [],
    imports: [
        SharedModule,
        SharedComponentsModule,
        DataTableSharedComponentModule,
        RouterModule.forChild(routes),
        ListModule,
    ],
})
export class DataTableModule { }
