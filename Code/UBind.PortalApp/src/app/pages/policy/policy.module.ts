import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BackNavigationGuard } from '@app/providers/guard/back-navigation.guard';
import { ListPolicyPage } from './list-policy/list-policy.page';
import { DetailPolicyPage } from './detail-policy/detail-policy.page';
import { UpdatePolicyPage } from './update-policy/update-policy.page';
import { DetailPolicyTransactionPage } from './detail-policy-transaction/detail-policy-transaction.page';
import { PermissionGuard } from '@app/providers/guard/permission.guard';
import { Permission } from '@app/helpers/permissions.helper';
import { ListPolicyTransactionPage } from './list-policy-transaction/list-policy-transaction.page';
import { ListNoSelectionPage } from '../list-no-selection/list-no-selection.page';
import { PolicyCommonModule } from './policy-common.module';
import { ShowMasterComponentWhenNotSplit }
    from '@app/components/show-master-when-not-split/show-master-when-not-split.component';
import { ListPolicyMessagePage } from './list-policy-message/list-policy-message.page';
import { DetailEmailPage } from '@app/pages/message/detail-email/detail-email.page';
import { ListPolicyTransactionMessagePage }
    from './list-policy-transaction-message/list-policy-transaction-message.page';
import { DetailSmsPage } from '../message/detail-sms/detail-sms.page';
import { MessageSharedComponentModule } from '../message/message-shared-component.module';
import { DetailClaimPage } from '../claim/detail-claim/detail-claim.page';
import { ListPolicyClaimPage } from './list-policy-claim/list-policy-claim.page';
import { PolicyFilterComponent } from '@app/components/filter/policy-filter.component';
import { ClaimSharedComponentModule } from '../claim/claim-shared-component.module';
import { TypedRoutes } from '@app/routing/typed-route';
import { AdditionalPropertyValueModule } from '../additional-property-values/additional-property-value.module';
import { UpdatePolicyNumberPage } from './update-policy-number/update-policy-number.page';

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
            mustHavePermissions:
                [Permission.ViewPolicies,
                    Permission.ViewAllPolicies,
                    Permission.ViewAllPoliciesFromAllOrganisations],
            masterComponent: ListPolicyPage,
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
            mustHavePermissions:
                [Permission.ViewPolicies,
                    Permission.ViewAllPolicies,
                    Permission.ViewAllPoliciesFromAllOrganisations],
            masterComponent: ListPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId',
        component: DetailPolicyPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewPolicies,
                    Permission.ViewAllPolicies,
                    Permission.ViewAllPoliciesFromAllOrganisations],
            masterComponent: ListPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/adjust/:quoteId',
        component: UpdatePolicyPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManagePolicies,
                    Permission.ManageAllPolicies,
                    Permission.ManageAllPoliciesForAllOrganisations],
            masterComponent: ListPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/cancel/:quoteId',
        component: UpdatePolicyPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManagePolicies,
                    Permission.ManageAllPolicies,
                    Permission.ManageAllPoliciesForAllOrganisations],
            masterComponent: ListPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/renew/:quoteId',
        component: UpdatePolicyPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManagePolicies,
                    Permission.ManageAllPolicies,
                    Permission.ManageAllPoliciesForAllOrganisations],
            masterComponent: ListPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/transaction/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [BackNavigationGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewPolicies,
                    Permission.ViewAllPolicies,
                    Permission.ViewAllPoliciesFromAllOrganisations],
            masterComponent: ListPolicyTransactionPage,
            masterContainerClass: 'master-list',
            detailComponent: ListNoSelectionPage,
            noSelectionMessage: 'Select a policy transaction to view details',
        },
    },
    {
        path: ':policyId/transaction/:policyTransactionId',
        component: DetailPolicyTransactionPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ViewPolicies,
                    Permission.ViewAllPolicies,
                    Permission.ViewAllPoliciesFromAllOrganisations],
            masterComponent: ListPolicyTransactionPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/email/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [
                Permission.ViewPolicies,
                Permission.ViewAllPolicies,
                Permission.ViewAllPoliciesFromAllOrganisations,
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
            mustHavePermissions: [
                Permission.ViewPolicies,
                Permission.ViewAllPolicies,
                Permission.ViewAllPoliciesFromAllOrganisations,
                Permission.ViewMessages,
                Permission.ViewAllMessages,
            ],
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
            mustHaveOneOfPermissions: [Permission.ViewMessages, Permission.ViewAllMessages],
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
        path: ':entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListPolicyPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/transaction/:entityId/additional-property-values',
        loadChildren: () => AdditionalPropertyValueModule,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [Permission.EditAdditionalPropertyValues],
            masterComponent: ListPolicyTransactionPage,
            masterContainerClass: 'master-list',
        },
        canDeactivate: [BackNavigationGuard],
    },
    {
        path: ':policyId/claim/list',
        component: ShowMasterComponentWhenNotSplit,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions: [
                Permission.ViewPolicies,
                Permission.ViewAllPolicies,
                Permission.ViewAllPoliciesFromAllOrganisations,
                Permission.ViewClaims,
                Permission.ViewAllClaims,
                Permission.ViewAllClaimsFromAllOrganisations,
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
            mustHavePermissions: [
                Permission.ViewPolicies,
                Permission.ViewAllPolicies,
                Permission.ViewAllPoliciesFromAllOrganisations,
                Permission.ViewClaims,
                Permission.ViewAllClaims,
                Permission.ViewAllClaimsFromAllOrganisations,
            ],
            masterComponent: ListPolicyClaimPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
    },
    {
        path: ':policyId/update-number/:policyNumber',
        component: UpdatePolicyNumberPage,
        canActivate: [PermissionGuard],
        data: {
            mustHavePermissions:
                [Permission.ManagePolicies, Permission.ManagePolicyNumbers],
            masterComponent: ListPolicyPage,
            masterContainerClass: 'master-list',
            onEnvironmentChangeRedirectTo: 'list',
        },
        canDeactivate: [BackNavigationGuard],
    },
];

/**
 * Export policy module class.
 * This class manage Ng Module declarations of
 * Policy.
 */
@NgModule({
    declarations: [
        ListPolicyPage,
    ],
    imports: [
        PolicyCommonModule,
        MessageSharedComponentModule,
        ClaimSharedComponentModule,
        RouterModule.forChild(routes),
    ],
})
export class PolicyModule { }
