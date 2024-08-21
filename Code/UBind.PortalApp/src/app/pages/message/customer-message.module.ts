import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { DetailEmailPage } from './detail-email/detail-email.page';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { ListMessageCustomerPage } from './list-message-customer/list-message-customer.page';
import { EnvironmentGuard } from '@app/providers/guard/environment.guard';
import { FilterSortPage } from '@app/pages/filter-sort/filter-sort-page';
import { DetailSmsPage } from './detail-sms/detail-sms.page';
import { MessageCommonModule } from './message-common.module';
import { TypedRoutes } from '@app/routing/typed-route';

const routes: TypedRoutes = [
    {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
    },
    {
        path: 'list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListMessageCustomerPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a message to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListMessageCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListMessageCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListMessageCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
];

/**
 * Export customer email module class.
 * TODO: Write a better class header: Ng Module declarations
 * in customer email.
 */
@NgModule({
    declarations: [
        ListMessageCustomerPage,
    ],
    imports: [
        MessageCommonModule,
        RouterModule.forChild(routes),
    ],
})
export class CustomerMessageModule { }
