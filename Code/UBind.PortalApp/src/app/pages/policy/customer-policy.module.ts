import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { PolicyCommonModule } from './policy-common.module';
import { ListCustomerPolicyPage, ListPolicyTransactionPage } from './policy.index';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { UpdatePolicyPage } from './update-policy/update-policy.page';
import { DetailPolicyTransactionPage } from './detail-policy-transaction/detail-policy-transaction.page';
import { DetailPolicyPage } from './detail-policy/detail-policy.page';
import { EnvironmentGuard } from '@app/providers/guard/environment.guard';
import { ListPolicyMessagePage } from './list-policy-message/list-policy-message.page';
import { ListPolicyTransactionMessagePage }
    from './list-policy-transaction-message/list-policy-transaction-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { ListPolicyClaimPage } from './list-policy-claim/list-policy-claim.page';
import { DetailClaimPage } from '../claim/detail-claim/detail-claim.page';
import { PolicyFilterComponent } from '@app/components/filter/policy-filter.component';
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
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListCustomerPolicyPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a policy to view details',
        },
    },
    {
        path: 'filter',
        component: PolicyFilterComponent,
        canActivate: [PermissionGuard],
        canDeactivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListCustomerPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId',
        component: DetailPolicyPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListCustomerPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListCustomerPolicyPage,
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/adjust/:quoteId',
        component: UpdatePolicyPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManagePolicies],
            masterComponent: ListCustomerPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/cancel/:quoteId',
        component: UpdatePolicyPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManagePolicies],
            masterComponent: ListCustomerPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/renew/:quoteId',
        component: UpdatePolicyPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ManagePolicies],
            masterComponent: ListCustomerPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/transaction/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [EnvironmentGuard, BackNavigationGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListPolicyTransactionPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a policy transaction to view details',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/transaction/:policyTransactionId',
        component: DetailPolicyTransactionPage,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListPolicyTransactionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/transaction/:entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [EnvironmentGuard, PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListPolicyTransactionPage,
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/message/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [BackNavigationGuard],
        data: {
            mustHavePermissions: [
                Permission.ViewPolicies,
                Permission.ViewMessages,
            ],
            masterComponent: ListPolicyMessagePage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a message to view details',
        },
    },
    {
        path: ':policyId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListPolicyMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewPolicies],
            masterComponent: ListPolicyMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/transaction/:policyTransactionId/message',
        component: DetailPolicyTransactionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListPolicyTransactionMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/transaction/:policyTransactionId/message/email/:id',
        component: DetailEmailPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListPolicyTransactionMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/transaction/:policyTransactionId/message/sms/:id',
        component: DetailSmsPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            masterComponent: ListPolicyTransactionMessagePage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/claim/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [
                Permission.ViewPolicies,
                Permission.ViewClaims,
            ],
            masterComponent: ListPolicyClaimPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select an claim to view details',
        },
    },
    {
        path: ':policyId/claim/:claimId',
        component: DetailClaimPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [
                    Permission.ViewPolicies,
                    Permission.ViewClaims,
                ],
            masterComponent: ListPolicyClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
];

/**
 * Export customer policy module class.
 * This class manage Ng Module declarations of
 * Customer Policy.
 */
@NgModule({
    declarations: [
        ListCustomerPolicyPage,
    ],
    imports: [
        PolicyCommonModule,
        MessageSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class CustomerPolicyModule { }
