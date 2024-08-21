import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '@app/shared.module';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { SharedComponentsModule } from '@app/components/shared-components.module';
import { CreateCustomerPage } from './create-customer/create-customer.page';
import { EditCustomerPage } from './edit-customer/edit-customer.page';
import { DetailCustomerPage } from './detail-customer/detail-customer.page';
import { ListCustomerPage } from './list-customer/list-customer.page';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { ListModule } from '@app/list.module';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import {
    ShowMasterComponentWhenNotSplit,
} from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { CreateEditCustomerPaymentPage } from './create-customer-payment/create-edit-customer-payment.component';
import { CreateEditCustomerRefundPage } from './create-customer-refund/create-edit-customer-refund.component';
import { FilterSortPage } from '@app/pages/filter-sort/filter-sort-page';
import { ListCustomerMessagePage } from './list-customer-message/list-customer-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { AssignAgentComponent } from './assign-agent/assign-agent.component';
import { DetailPersonPage } from '../person/detail-person/detail-person.page';
import { ListPersonPage } from '../person/list-person/list-person.page';
import { CreateEditPersonPage } from '../person/create-edit-person/create-edit-person.page';
import { AssignCustomerPortalComponent } from './assign-customer-portal/assign-customer-portal.component';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { TypedRoutes } from '@app/routing/typed-route';
import { AdditionalPropertyValueModule } from '../additional-property-values/additional-property-value.module';

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
            mustHavePermissions: [Permission.ViewCustomers, Permission.ViewAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a customer to view details',
        },
    },
    {
        path: 'filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewCustomers, Permission.ViewAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: 'create',
        component: CreateCustomerPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageCustomers, Permission.ManageAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId/payment/create',
        component: CreateEditCustomerPaymentPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageCustomers, Permission.ManageAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId/refund/create',
        component: CreateEditCustomerRefundPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageCustomers, Permission.ManageAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId/edit',
        component: EditCustomerPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageCustomers, Permission.ManageAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId/assign-agent',
        component: AssignAgentComponent,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId/assign-portal',
        component: AssignCustomerPortalComponent,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManageCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId',
        component: DetailCustomerPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewCustomers, Permission.ViewAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':customerId/:segment',
        component: DetailCustomerPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewCustomers, Permission.ViewAllCustomers],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':customerId/message/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListCustomerMessagePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a message to view details',
        },
    },
    {
        path: ':customerId/message/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListCustomerMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':customerId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListCustomerMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':customerId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListCustomerMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':customerId/person/filter',
        component: FilterSortPage,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewCustomers, Permission.ViewAllCustomers],
            masterComponent: ListPersonPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: ':customerId/person/list',
        },
    },
    {
        path: ':customerId/person/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewCustomers, Permission.ViewAllCustomers],
            masterComponent: ListPersonPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a person to view details',
        },
    },
    {
        path: ':customerId/person/create',
        component: CreateEditPersonPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageCustomers],
            masterComponent: ListPersonPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: ':customerId/person/list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId/person/:personId/edit',
        component: CreateEditPersonPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ManageCustomers],
            masterComponent: ListPersonPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: ':customerId/person/list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':customerId/person/:personId',
        component: DetailPersonPage,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.ViewCustomers],
            masterComponent: ListPersonPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: ':customerId/person/list',
        },
    },
    {
        path: ':entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [PermissionGuard],
        data: {
            mustHaveOneOfPermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListCustomerPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export customer module class.
 * TODO: Write a better class header: ng module declarations.
 */
@NgModule({
    declarations: [
        DetailCustomerPage,
        EditCustomerPage,
        CreateCustomerPage,
        ListCustomerPage,
        CreateEditCustomerPaymentPage,
        CreateEditCustomerRefundPage,
        ListCustomerMessagePage,
        AssignAgentComponent,
        AssignCustomerPortalComponent,
        ListPersonPage,
        DetailPersonPage,
        CreateEditPersonPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
        MessageSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class CustomerModule { }
