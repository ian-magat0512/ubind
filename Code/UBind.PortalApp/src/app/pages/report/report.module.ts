import { NgModule } from '@angular/core';
import { ListReportPage } from './list-report/list-report.page';
import { DetailReportPage } from './detail-report/detail-report.page';
import { SharedModule } from '@app/shared.module';
import { CreateEditReportPage } from './create-edit-report/create-edit-report.page';
import { RouterModule, Routes } from '@angular/router';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { ReactiveFormsModule } from '@angular/forms';
import { GenerateReportPage } from './generate-report/generate-report.page';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { ListModule } from '../../list.module';
import { FilterSortPage } from '@pages/filter-sort/filter-sort-page';

const reportRoutes: Routes = [
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
            mustHavePermissions: [Permission.ViewReports],
            masterComponent: ListReportPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a report to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewReports],
            masterComponent: ListReportPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateEditReportPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageReports],
            masterComponent: ListReportPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':reportId/edit',
        component: CreateEditReportPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageReports],
            masterComponent: ListReportPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':reportId/generate',
        component: GenerateReportPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.GenerateReports],
            masterComponent: ListReportPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':reportId',
        component: DetailReportPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewReports],
            masterComponent: ListReportPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
];

/**
 * Export report module class.
 * This class manage Ng Module declarations of report.
 */
@NgModule({
    declarations: [
        ListReportPage,
        DetailReportPage,
        CreateEditReportPage,
        GenerateReportPage,
    ],
    imports: [
        ReactiveFormsModule,
        SharedModule,
        SharedComponentsModule,
        ListModule,
        RouterModule.forChild(reportRoutes),
    ],
})
export class ReportModule { }
